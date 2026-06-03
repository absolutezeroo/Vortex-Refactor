using System;
using System.Linq;

using Godot;
using Godot.Collections;

using Vortex.Core;
using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Communication.Login;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Outgoing.Handshake;
using Vortex.Habbo.Utils;
using Vortex.IID;

namespace Vortex.Habbo.Communication.Demo;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as
public class HabboCommunicationDemo : Component
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::ERROR_TYPE_IO_ERROR
    public const string ERROR_TYPE_IO_ERROR = "ioError";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::ERROR_CODE_MAINTENANCE
    public const string ERROR_CODE_MAINTENANCE = "maintenance";

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::AUTO_RECONNECT
    private const bool AUTO_RECONNECT = false;
    private readonly bool _autoLogin;

    private string _loginName = string.Empty;
    private HabboLoginDemoScreen? _view;
    private ILoginProvider? var_118;
    private string var_1306 = string.Empty;
    private bool var_1607;
    private bool var_1613;
    private string var_1998 = string.Empty;
    private bool var_2788;
    private IncomingMessages? var_2938;
    private string var_3649 = string.Empty;
    private string var_937 = string.Empty;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::HabboCommunicationDemo
    protected HabboCommunicationDemo(IContext param1, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        param1.events.AddEventListener("unload", Unloading);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::get dependencies
    protected override IList<ComponentDependency> dependencies =>
        base.dependencies.Concat(
                [
                    new ComponentDependency(
                        new IIDHabboWindowManager(),
                        param1 => windowManager = param1, !isRoomViewerMode
                    ),
                    new ComponentDependency(
                        new IIDHabboCommunicationManager(),
                        param1 => communication = param1 as IHabboCommunicationManager
                    ),
                    new ComponentDependency(
                        new IIDHabboLocalizationManager(),
                        param1 => localization = param1, true,
                        [
                            new DependencyEventListener("complete", OnLocalizationsComplete),
                            new DependencyEventListener("LOCALIZATION_EVENT_LOCALIZATION_FAILED", OnLocalizationFailed),
                        ]
                    ),
                ]
            )
            .ToList();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::get communication
    public IHabboCommunicationManager? communication { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::get localization
    public object? localization { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::get windowManager
    public object? windowManager { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::set ssoTicket
    public string ssoTicket
    {
        set => var_1998 = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::set flashClientUrl
    public string flashClientUrl
    {
        set => var_3649 = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::get isRoomViewerMode
    // Godot/C# adaptation: HabboComponentFlags not ported — hardcoded to false.
    public static bool isRoomViewerMode => false;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::initComponent
    protected override void InitComponent()
    {
        try
        {
            context.events.AddEventListener(COMPONENT_EVENT_ERROR, OnCoreError);

            var_1607 = false;

            if (var_2938 != null)
            {
                var_2938.Dispose();
                communication?.RenewSocket();
            }

            var_2938 = new IncomingMessages(this, communication!);

            context.events.AddEventListener("HHVE_ERROR", OnHotelViewError);

            PrepareProperties();

            if (_autoLogin)
            {
                InitWithStoredCredentials();
            }
            else if (!string.IsNullOrEmpty(var_1998))
            {
                InitWithSso(var_1998);
            }
            else
            {
                InitWithLoginView();
            }
        }
        catch (Exception e)
        {
            Logger.Error("Failed to initialize HabboCommunicationDemo", e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::dispose
    public override void Dispose()
    {
        // Godot/C# adaptation: class_79.instance is private — use context.events instead.
        context.events.RemoveEventListener(COMPONENT_EVENT_ERROR, OnCoreError);

        if (var_118 != null)
        {
            var_118.SsoTokenAvailable -= OnSsoTokenAvailable;
            var_118 = null;
        }

        if (_view != null)
        {
            _view.Events.RemoveEventListener(HabboLoginDemoScreen.INIT_LOGIN, OnInitLogin);
            _view.Events.RemoveEventListener(HabboLoginDemoScreen.AVATAR_SELECTED, OnAvatarSelected);
            _view.Events.RemoveEventListener(HabboLoginDemoScreen.ENVIRONMENT_SELECTED, OnEnvironmentSelected);
            _view.Dispose();
            _view = null;
        }

        if (var_2938 != null)
        {
            var_2938.Dispose();
            var_2938 = null;
        }

        localization = null;
        communication = null;

        base.Dispose();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::initGameSocket
    public void InitGameSocket()
    {
        DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_INIT");

        if (communication == null)
        {
            return;
        }

        communication.mode = 0;
        communication.InitConnection("habbo");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::setSSOTicket
    public void SetSsoTicket(string param1)
    {
        if (string.IsNullOrEmpty(param1) || !string.IsNullOrEmpty(var_1998))
        {
            return;
        }

        var_1998 = param1;

        InitGameSocket();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::sendTryLoginDevelopmentOnly
    public void sendTryLoginDevelopmentOnly(string param1, string param2, int param3 = 0)
    {
        if (communication?.connection == null)
        {
            communication?.InitConnection("habbo");
        }

        communication?.connection?.Send(new Class3383(param1, param2, param3));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::sendConnectionParameters
    public void SendConnectionParameters(IConnection param1)
    {
        // Godot/C# adaptation: CommunicationUtils.readSOLString not ported — use placeholder.
        string machineId = CommunicationUtils.ReadSOLString("machineid") ?? string.Empty;
        string fingerprint = CommunicationUtils.GenerateFingerprint();
        string capabilities = OS.GetName() + "/" + Engine.GetVersionInfo()["string"].AsString();

        param1.Send(new UniqueIdMessageComposer(machineId, fingerprint, capabilities));

        if (!string.IsNullOrEmpty(var_1998))
        {
            param1.Send(new SsoTicketMessageComposer(var_1998));
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::loginOk
    public void LoginOk()
    {
        var_1607 = false;

        if (_view != null)
        {
            HabboLoginDemoScreen.CloseLoginWindow();
            _view.Dispose();
            _view = null;
        }

        var_1613 = false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::alert
    public static void Alert(string param1, string param2)
    {
        // TODO(as3-port): Use _windowManager.alert() once IHabboWindowManager is ported.
        Logger.Warn($"ALERT: {param1} {param2}");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::dispatchLoginStepEvent
    public void DispatchLoginStepEvent(string param1)
    {
        if (context is not Component comp)
        {
            return;
        }

        comp.events.DispatchEvent(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onUserList
    public static void OnUserList(Array<Dictionary> param1) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::disconnected
    public void Disconnected(int param1, string param2)
    {
        var_1607 = true;

        if (_view == null)
        {
            Logger.Warn($"Disconnected reason={param1} name={param2}");
            return;
        }

        OnBufferedDisconnected(param1, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::handleErrorMessage
    public void HandleErrorMessage(int param1, int param2)
    {
        switch (param1)
        {
            case 0:
                Alert("${connection.server.error.title}", "${connection.server.error.desc}");
                break;
            case >= 1001 and <= 1019:
                communication?.connection?.Close();
                break;
            case 4013:
                Alert("${connection.room.maintenance.title}", "${connection.room.maintenance.desc}");
                break;
            default:
                Alert("${connection.server.error.title}", "${connection.server.error.desc}");
                break;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::handleLoginFailedHotelClosedMessage
    public void HandleLoginFailedHotelClosedMessage(int param1, int param2)
    {
        _view?.ShowDisconnectedWithText(12);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::prepareProperties
    private void PrepareProperties()
    {
        // Godot/C# adaptation: CommunicationUtils.readSOLString/restorePassword not ported.
        _loginName = string.Empty;
        var_937 = string.Empty;

        var_1998 = GetProperty("sso.token");
        var_3649 = GetProperty("flash.client.url");
        var_1306 = GetProperty("external.variables.txt");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::initWithStoredCredentials
    private void InitWithStoredCredentials()
    {
        if (communication == null)
        {
            return;
        }

        communication.mode = 0;

        // Godot/C# adaptation: CommunicationUtils.readSOLString("environment") not ported.
        InitEnvironment("default");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::initWithSSO
    private void InitWithSso(string param1)
    {
        if (communication == null)
        {
            return;
        }

        var_1998 = param1;
        communication.mode = 0;

        InitGameSocket();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::initWithLoginView
    private void InitWithLoginView()
    {
        if (windowManager != null && !isRoomViewerMode)
        {
            CoreEnvironment.Crash("Login without an SSO ticket is not supported", 29);
        }

        _view = new HabboLoginDemoScreen(key => GetProperty(key));
        var_118 = new WebApiLoginProvider(_view);
        var_118.SsoTokenAvailable += OnSsoTokenAvailable;

        _view.Events.AddEventListener(HabboLoginDemoScreen.INIT_LOGIN, OnInitLogin);
        _view.Events.AddEventListener(HabboLoginDemoScreen.AVATAR_SELECTED, OnAvatarSelected);
        _view.Events.AddEventListener(HabboLoginDemoScreen.ENVIRONMENT_SELECTED, OnEnvironmentSelected);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::initEnvironment
    private void InitEnvironment(string param1)
    {
        SetProperty("environment.id", param1);
        UpdateEnvironmentVariables(param1);

        communication?.UpdateHostParameters();

        if (_view == null)
        {
            return;
        }

        if (_view.UseWebApi)
        {
            var_118?.Init(communication);
        }
        else
        {
            _view.EnvironmentReady();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::userExists
    public static bool UserExists(Array<Dictionary> param1, string param2)
    {
        foreach (Dictionary row in param1)
        {
            if (row.TryGetValue("uniqueId", out Variant value) && value.AsString() == param2)
            {
                return true;
            }
        }

        return false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onBufferedDisconnected
    private void OnBufferedDisconnected(int param1, string param2)
    {
        if (param1 == 20)
        {
            _view?.ShowInvalidLoginError(null);
            return;
        }

        _view?.ShowDisconnected(param1, param2);
        communication?.CloseConnection();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::updateEnvironmentVariables
    private void UpdateEnvironmentVariables(string param1)
    {
        string[] keys =
        [
            "connection.info.host",
            "connection.info.port",
            "url.prefix",
            "site.url",
            "flash.dynamic.download.url",
            "flash.dynamic.download.name.template",
            "flash.dynamic.avatar.download.configuration",
            "flash.dynamic.avatar.download.url",
            "pocket.api",
            "web.api",
            "facebook.application.id",
            "web.terms_of_service.link",
        ];

        foreach (string key in keys)
        {
            string value = GetProperty(key);
            string envKey = key + "." + param1;

            if (PropertyExists(envKey))
            {
                SetProperty(key, GetProperty(envKey));
            }
            else
            {
                SetProperty(key, value);
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onLocalizationsComplete
    private void OnLocalizationsComplete(object? param1) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onLocalizationFailed
    private void OnLocalizationFailed(object? param1) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::unloading
    private void Unloading(object? param1)
    {
        var_2788 = true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onCoreError
    public void OnCoreError(object? param1)
    {
        // Godot/C# adaptation: ErrorEvent not ported — simplified handler.
        Disconnected(-1, DisconnectReasonEvent.ResolveDisconnectedReasonLocalizationKey(-1));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onHotelViewError
    public void OnHotelViewError(object? param1)
    {
        Disconnected(-2, "${disconnected.maintenance}");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onInitLogin
    private void OnInitLogin(object? param1)
    {
        if (_view == null)
        {
            return;
        }

        if (_view.UseWebApi)
        {
            var_118?.LoginWithCredentials(_view.LoginName, _view.Password);
        }
        else
        {
            InitGameSocket();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onAvatarSelected
    private void OnAvatarSelected(object? param1)
    {
        if (_view == null)
        {
            return;
        }

        if (_view.UseWebApi && _view.SelectedAccount != null)
        {
            var_118?.SelectAvatarUniqueid(_view.SelectedAccount.UniqueId);
        }
        else
        {
            sendTryLoginDevelopmentOnly(_view.LoginName, _view.Password, _view.AvatarId);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/HabboCommunicationDemo.as::onEnvironmentSelected
    private void OnEnvironmentSelected(object? param1)
    {
        if (_view != null)
        {
            InitEnvironment(_view.SelectedEnvironment);
        }
    }

    /// Godot/C# adaptation: Handle SSO token from ILoginProvider.
    private void OnSsoTokenAvailable(SsoTokenAvailableEvent args)
    {
        SetSsoTicket(args.SsoToken);
    }
}
