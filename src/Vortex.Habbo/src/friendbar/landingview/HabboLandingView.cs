// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/landingview/HabboLandingView.as

using System;
using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Core.Window;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Communication.Messages.Incoming.Navigator;
using Vortex.Habbo.Window;
using Vortex.Habbo.Window.Utils;
using Vortex.IID;

namespace Vortex.Habbo.FriendBar.LandingView;

/// @see com.sulake.habbo.friendbar.landingview.HabboLandingView
public class HabboLandingView : Component, IHabboLandingView
{
    public const uint LANDING_VIEW_LAYER = 1;

    private IHabboCommunicationManager? _communicationManager;
    private IHabboWindowManager? _windowManager;
    private WidgetContainerLayout? _landingViewLayout;
    private IMessageEvent? _navigatorSettingsEvent;
    private bool _initialized;

    /// @see HabboLandingView.as::HabboLandingView
    public HabboLandingView(IContext param1, uint param2 = 0, object? param3 = null) : base(param1, param2, param3)
    {
        RegisterInterface(new IIDHabboLandingView(), this);
    }

    public IHabboWindowManager WindowManager => _windowManager!;

    /// @see HabboLandingView.as::dependencies
    protected override IList<ComponentDependency> dependencies =>
        base.dependencies.Concat(
                [
                    new ComponentDependency(
                        new IIDHabboCommunicationManager(), param1 => _communicationManager = param1 as IHabboCommunicationManager),
                    new ComponentDependency(new IIDHabboWindowManager(), param1 => _windowManager = param1 as IHabboWindowManager),
                ]
            )
            .ToList();

    /// @see HabboLandingView.as::initComponent
    protected override void InitComponent()
    {
        if (_communicationManager != null)
        {
            _navigatorSettingsEvent = _communicationManager.AddHabboConnectionMessageEvent(
                new NavigatorSettingsEvent(OnNavigatorSettings)
            );
        }

        Logger.Info("[HabboLandingView] Initialized and waiting for NavigatorSettings");

        events.DispatchEvent("complete");
    }

    /// @see HabboLandingView.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (_communicationManager != null && _navigatorSettingsEvent != null)
        {
            _communicationManager.RemoveHabboConnectionMessageEvent(_navigatorSettingsEvent);
            _navigatorSettingsEvent = null;
        }

        _landingViewLayout?.Dispose();
        _landingViewLayout = null;
        _windowManager = null;
        _communicationManager = null;
        _initialized = false;

        base.Dispose();
    }

    /// @see HabboLandingView.as::initialize
    public void Initialize()
    {
        _initialized = true;

        RemoveWelcomeWindow();

        _landingViewLayout = new WidgetContainerLayout(this);
        Activate();
    }

    /// @see IHabboLandingView.as::activate
    public void Activate()
    {
        if (!_initialized)
        {
            TryInitialize();
        }

        if (_landingViewLayout != null)
        {
            _landingViewLayout.Activate();
        }
        else
        {
            Logger.Warn("[HabboLandingView] Landing view layout is not initialized and cannot be activated");
        }
    }

    /// @see IHabboLandingView.as::disable
    public void Disable()
    {
        _landingViewLayout?.Disable();
    }

    /// @see HabboLandingView.as::getXmlWindow
    public IWindow? GetXmlWindow(string param1, uint param2 = 1)
    {
        XElement? xml = ResolveXmlAsset(param1 + "_xml");

        if (xml == null)
        {
            return null;
        }

        return ((IWindowFactory)WindowManager).BuildFromXml(xml, param2);
    }

    /// @see HabboLandingView.as::onNavigatorSettings
    private void OnNavigatorSettings(IMessageEvent param1)
    {
        if (param1 is not NavigatorSettingsEvent navigatorSettingsEvent)
        {
            return;
        }

        if (navigatorSettingsEvent.RoomIdToEnter <= 0)
        {
            TryInitialize();
        }
    }

    /// @see HabboLandingView.as::tryInitialize
    private void TryInitialize()
    {
        try
        {
            Initialize();
        }
        catch (Exception error)
        {
            _landingViewLayout?.Dispose();
            _landingViewLayout = null;
            _initialized = false;

            Logger.Error("[HabboLandingView] Landing view layout initialization failed", error);
        }
    }

    private void RemoveWelcomeWindow()
    {
        IWindow? desktop = WindowManager.GetWindowContext(LANDING_VIEW_LAYER)?.GetDesktopWindow();
        IWindow? welcomeWindow = desktop?.GetChildByName("hotel_view_welcome_window");

        if (desktop != null && welcomeWindow != null)
        {
            desktop.RemoveChild(welcomeWindow);
            welcomeWindow.Destroy();
        }
    }

    private XElement? ResolveXmlAsset(string assetName)
    {
        if (assets is IAssetLibrary assetLibrary)
        {
            IAsset? asset = assetLibrary.GetAssetByName(assetName);

            if (asset?.Content is XElement xml)
            {
                return xml;
            }

            if (asset?.Content is XDocument { Root: not null } document)
            {
                return document.Root;
            }

            if (asset?.Content is string xmlString)
            {
                return XElement.Parse(xmlString);
            }
        }

        return HabboAssetResolver.LoadXmlAsset(assetName);
    }
}
