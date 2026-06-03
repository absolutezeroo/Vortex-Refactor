// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Utils;
using Vortex.IID;

using Timer = Godot.Timer;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as
public partial class HabboAirMain : Control
{
    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::CORE_RATIO
    private const double CORE_RATIO = 0.6;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::INIT_STEPS
    private const int INIT_STEPS = 3;
    private readonly Dictionary<string, object?> safeStr_245 = new(StringComparer.Ordinal);
    private int _completedInitSteps;
    private bool _configurationReady;

    private ICore? _core;
    private bool _coreRunningReady;
    private EventDispatcherWrapper? _loadingEventDispatcher;
    private IHabboLoadingScreen? _loadingScreen;
    private bool _localizationReady;
    private bool _prepareCoreOnNextFrame;
    private bool _roomEngineReady;
    private bool safeStr_412;
    private bool safeStr_413;
    private int safeStr_414;
    private int safeStr_415 = INIT_STEPS;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::HabboAirMain
    public HabboAirMain() { }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::HabboAirMain
    public HabboAirMain(IHabboLoadingScreen? param1, Dictionary<string, object?>? param2)
    {
        _loadingScreen = param1;

        if (param2 != null)
        {
            safeStr_245 = Clone(param2);
        }

        Logger.Info($"Core version: {CoreEnvironment.version}");
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::_Ready
    public override void _Ready()
    {
        Logger.Initialize();

        CoreEnvironment.ExternalLogWarn = HabboWebTools.LogWarn;
        CoreEnvironment.ExternalLogDebug = HabboWebTools.LogDebug;

        OnAddedToStage();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::_Process
    public override void _Process(double delta)
    {
        _ = delta;
        OnExitFrame();
    }

    /// Desktop equivalent of ExternalInterface.addCallback("unloading", unloading).
    /// Godot fires NotificationWMCloseRequest when the window close button is pressed.
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Unloading();
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::dispose
    private new void Dispose()
    {
        if (_loadingEventDispatcher != null)
        {
            _loadingEventDispatcher.RemoveEventListener("progress", OnProgressEvent);
            _loadingEventDispatcher.RemoveEventListener("complete", OnCompleteEvent);
            _loadingEventDispatcher = null;
        }

        if (_core != null)
        {
            _core.events.RemoveEventListener(Component.COMPONENT_EVENT_ERROR, OnCoreError);
            _core.events.RemoveEventListener(Component.COMPONENT_EVENT_REBOOT, OnCoreReboot);
            _core.events.RemoveEventListener(Component.COMPONENT_EVENT_RUNNING, OnCoreRunning);
        }

        _loadingScreen?.Dispose();
        _loadingScreen = null;
        _core = null;

        Logger.Shutdown();

        GetParent()?.RemoveChild(this);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::unloading
    public void Unloading()
    {
        try
        {
            if (_core is { disposed: false })
            {
                _core.events.DispatchEvent("unload");
            }
        }
        catch
        {
            // Ignore unloading errors exactly like AS3 flow.
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onAddedToStage
    protected void OnAddedToStage()
    {
        try
        {
            Init();
        }
        catch (Exception error)
        {
            HabboAir.TrackLoginStep("client.init.core.fail");
            HabboAir.ReportCrash($"Failed to prepare the core: {error.Message}", CoreEnvironment.ERROR_CATEGORY_INITIALIZE_CORE, true,
                error);
            CoreEnvironment.Dispose();
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::init
    private void Init()
    {
        _prepareCoreOnNextFrame = true;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onExitFrame
    protected void OnExitFrame()
    {
        if (_prepareCoreOnNextFrame)
        {
            _prepareCoreOnNextFrame = false;

            PrepareCore();

            return;
        }

        if (safeStr_412 && safeStr_413)
        {
            Dispose();
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    private void PrepareCore()
    {
        try
        {
            HabboCoreErrorReporter errorReporter = new();
            _core = CoreEnvironment.Instantiate(this, CoreEnvironment.CORE_SETUP_FRAME_UPDATE_COMPLEX, errorReporter, safeStr_245);
            _core.events.AddEventListener(Component.COMPONENT_EVENT_ERROR, OnCoreError);
            _core.events.AddEventListener(Component.COMPONENT_EVENT_REBOOT, OnCoreReboot);
            _core.events.AddEventListener(Component.COMPONENT_EVENT_RUNNING, OnCoreRunning);

            _loadingEventDispatcher = new EventDispatcherWrapper();
            _loadingEventDispatcher.AddEventListener("progress", OnProgressEvent);
            _loadingEventDispatcher.AddEventListener("complete", OnCompleteEvent);

            XElement configXml = new(
                "config",
                new XElement("asset-libraries"),
                new XElement("service-libraries"),
                new XElement("component-libraries")
            );

            _core.ReadConfigDocument(configXml, _loadingEventDispatcher);

            if (_core is CoreComponentContext coreComponentContext)
            {
                // TODO(window-port): Port AIR FileProxy and assign it here.
                coreComponentContext.fileProxy = null;
            }

            safeStr_415 = (int)(_core.GetNumberOfFilesPending() + _core.GetNumberOfFilesLoaded()) + INIT_STEPS;

            PrepareBootstrapComponents();
            AddInitializationProgressListeners();
        }
        catch (Exception error)
        {
            Logger.Error("Failed to prepare core", error);
            CoreEnvironment.Dispose();

            _core = null;
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::updateProgressBar
    private void UpdateProgressBar()
    {
        if (_loadingScreen == null)
        {
            return;
        }

        double ratio = CORE_RATIO + ((_completedInitSteps + safeStr_414) / (double)Math.Max(1, safeStr_415) * (1 - CORE_RATIO));
        _loadingScreen.UpdateLoadingBar(ratio);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onProgressEvent
    private void OnProgressEvent(object? param1 = null)
    {
        _ = param1;

        if (_core == null)
        {
            return;
        }

        safeStr_414 = (int)_core.GetNumberOfFilesLoaded();

        UpdateProgressBar();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onCompleteEvent
    private void OnCompleteEvent(object? param1 = null)
    {
        _ = param1;
        if (_loadingEventDispatcher != null)
        {
            _loadingEventDispatcher.RemoveEventListener("progress", OnProgressEvent);
            _loadingEventDispatcher.RemoveEventListener("complete", OnCompleteEvent);
            _loadingEventDispatcher = null;
        }

        InitializeCore();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::initializeCore
    private void InitializeCore()
    {
        HabboAir.TrackLoginStep("client.init.core.init");

        try
        {
            _core?.Initialize();
        }
        catch (Exception error)
        {
            HabboAir.TrackLoginStep("client.init.core.fail");
            CoreEnvironment.Crash($"Failed to initialize the core: {error.Message}", CoreEnvironment.ERROR_CATEGORY_INITIALIZE_CORE, error);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onCoreError
    public static void OnCoreError(object? param1)
    {
        Logger.Error($"onCoreError {param1}");
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onCoreReboot
    private void OnCoreReboot(object? param1 = null)
    {
        _ = param1;
        Logger.Warn("Reboot application!");
        if (_core != null)
        {
            _core.events.RemoveEventListener(Component.COMPONENT_EVENT_ERROR, OnCoreError);
            _core.events.RemoveEventListener(Component.COMPONENT_EVENT_REBOOT, OnCoreReboot);
        }

        CoreEnvironment.Dispose();

        _core = null;

        Logger.Info("Application ready for restart!");
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::simpleQueueInterface
    private void SimpleQueueInterface(Core.Runtime.IID param1, Action<Core.Runtime.IID, IUnknown?> param2)
    {
        bool callbackInvoked = false;

        IUnknown? queuedInterface = _core?.QueueInterface(param1, CallbackProxy);

        if (queuedInterface != null && !callbackInvoked)
        {
            param2(param1, queuedInterface);
        }
        return;

        void CallbackProxy(Core.Runtime.IID iid, IUnknown? unknown)
        {
            callbackInvoked = true;
            param2(iid, unknown);
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::addInitializationProgressListeners
    private void AddInitializationProgressListeners()
    {
        SimpleQueueInterface(
            new IIDHabboLocalizationManager(), (_, param2) =>
            {
                if (param2 is Component component)
                {
                    component.events.AddEventListener("complete", OnLocalizationComplete);
                }

                if (param2 is HabboBootstrapComponentBase { isInitialized: true })
                {
                    OnLocalizationComplete();
                }
            }
        );

        SimpleQueueInterface(new IIDHabboConfigurationManager(), OnConfigurationComplete);

        SimpleQueueInterface(
            new IIDRoomEngine(), (_, param2) =>
            {
                if (param2 is Component component)
                {
                    component.events.AddEventListener("REE_ENGINE_INITIALIZED", OnRoomEngineReady);
                }

                if (param2 is HabboBootstrapComponentBase { isInitialized: true })
                {
                    OnRoomEngineReady();
                }
            }
        );
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::prepareCore
    private void PrepareBootstrapComponents()
    {
        if (_core == null)
        {
            return;
        }

        // @see ComponentContext.as — AS3's getDefinitionByName searches the entire ApplicationDomain.
        // In Godot .NET, assemblies are loaded in a custom AssemblyLoadContext not visible to
        // Assembly.Load. Register each project assembly explicitly so manifest type resolution
        // can find bootstrap classes and IID types across all projects.
        ComponentContext.RegisterManifestAssembly(typeof(ComponentContext).Assembly);                         // Vortex.Core
        ComponentContext.RegisterManifestAssembly(typeof(HabboTrackingLib).Assembly);                         // Vortex.Bootstrap
        ComponentContext.RegisterManifestAssembly(typeof(IIDCoreCommunicationManager).Assembly);              // Vortex.IID
        ComponentContext.RegisterManifestAssembly(typeof(Habbo.Window.HabboWindowManagerComponent).Assembly); // Vortex.Habbo


        Type[] componentChain =
        [
            typeof(HabboTrackingLib),
            typeof(CoreCommunicationFrameworkLib),
            typeof(HabboRoomObjectLogicLib),
            typeof(HabboRoomObjectVisualizationLib),
            typeof(RoomManagerLib),
            typeof(RoomSpriteRendererLib),
            typeof(HabboRoomSessionManagerLib),
            typeof(HabboAvatarRenderLib),
            typeof(HabboSessionDataManagerLib),
            typeof(HabboConfigurationCom),
            typeof(HabboLocalizationCom),
            typeof(HabboWindowManagerCom),
            typeof(HabboCommunicationCom),
            typeof(HabboCommunicationDemoCom),
            typeof(HabboNavigatorCom),
            typeof(HabboFriendListCom),
            typeof(HabboMessengerCom),
            typeof(HabboInventoryCom),
            typeof(HabboToolbarCom),
            typeof(HabboCatalogCom),
            typeof(HabboRoomEngineCom),
            typeof(HabboRoomUICom),
            typeof(HabboAvatarEditorCom),
            typeof(HabboNotificationsCom),
            typeof(HabboHelpCom),
            typeof(HabboAdManagerCom),
            typeof(HabboModerationCom),
            typeof(HabboUserDefinedRoomEventsCom),
            typeof(HabboSoundManagerFlash10Com),
            typeof(HabboQuestEngineCom),
            typeof(HabboFriendBarCom),
            typeof(HabboGroupsCom),
            typeof(HabboGamesCom),
            typeof(HabboFreeFlowChatCom),
            typeof(HabboNewNavigatorCom),
        ];

        foreach (Type componentType in componentChain)
        {
            try
            {
                _core.PrepareComponent(componentType);
            }
            catch (Exception error)
            {
                Logger.Error($"[PrepareCore] {componentType.Name} FAILED: {error.Message}", error);

                HabboAir.ReportCrash(
                    $"Failed to prepare component {componentType.Name}: {error.Message}",
                    CoreEnvironment.ERROR_CATEGORY_PREPARE_COMPONENT, false, error
                );
            }
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onLocalizationComplete
    private void OnLocalizationComplete(object? param1 = null)
    {
        _ = param1;

        if (_localizationReady)
        {
            return;
        }
        _localizationReady = true;

        HabboAir.TrackLoginStep("client.init.localization.loaded");

        _completedInitSteps++;

        UpdateProgressBar();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onConfigurationComplete
    private void OnConfigurationComplete(Core.Runtime.IID param1, IUnknown? param2)
    {
        _ = param1;
        _ = param2;

        if (_configurationReady)
        {
            return;
        }
        _configurationReady = true;

        HabboAir.TrackLoginStep("client.init.config.loaded");

        _completedInitSteps++;

        UpdateProgressBar();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onRoomEngineReady
    private void OnRoomEngineReady(object? param1 = null)
    {
        _ = param1;

        if (_roomEngineReady)
        {
            return;
        }
        _roomEngineReady = true;

        safeStr_412 = true;

        HabboAir.TrackLoginStep("client.init.room.ready");

        if (_core != null && _core.GetInteger("spaweb", 0) == 1)
        {
            StartSendingHeartBeat();
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::startSendingHeartBeat
    private void StartSendingHeartBeat()
    {
        SendHeartBeat();

        Timer timer = new()
        {
            WaitTime = 10.0,
            OneShot = false,
            Autostart = true,
        };

        timer.Timeout += SendHeartBeat;

        AddChild(timer);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::sendHeartBeat
    private static void SendHeartBeat()
    {
        HabboWebTools.SendHeartBeat();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::onCoreRunning
    private void OnCoreRunning(object? param1 = null)
    {
        _ = param1;

        if (_coreRunningReady)
        {
            return;
        }
        _coreRunningReady = true;

        safeStr_413 = true;

        HabboAir.TrackLoginStep("client.init.core.running");

        _completedInitSteps++;

        UpdateProgressBar();
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::getInteger
    private int GetInteger(string param1, int param2 = 0)
    {
        if (safeStr_245.TryGetValue(param1, out object? value) && value != null && int.TryParse(Convert.ToString(value), out int parsed))
        {
            return parsed;
        }

        return param2;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::clone
    private static Dictionary<string, object?> Clone(Dictionary<string, object?> param1)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, object?> pair in param1)
        {
            if (pair.Value is Dictionary<string, object?> nested)
            {
                result[pair.Key] = Clone(nested);
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }
}

/// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::HabboCoreErrorReporter
internal sealed class HabboCoreErrorReporter : ICoreErrorReporter
{
    private ICoreErrorLogger? _logger;

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::HabboCoreErrorReporter::logError
    public void LogError(string param1, bool param2, int param3 = -1, Exception? param4 = null)
    {
        _logger?.LogError(param1);

        HabboAir.ReportCrash(param1, param3, param2, param4, _logger);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/HabboAirMain.as::HabboCoreErrorReporter::set errorLogger
    public ICoreErrorLogger? errorLogger
    {
        set => _logger = value;
    }
}
