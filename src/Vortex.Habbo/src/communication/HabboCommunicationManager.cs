using System;
using System.Linq;
using System.Timers;

using Godot.Collections;

using Vortex.Core.Communication;
using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Handshake;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Utils;
using Vortex.Habbo.Communication.Encryption;
using Vortex.Habbo.Communication.Messages.Outgoing.Handshake;
using Vortex.Habbo.Configuration;
using Vortex.IID;

using Array = Godot.Collections.Array;
using Timer = System.Timers.Timer;

namespace Vortex.Habbo.Communication;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as
public class HabboCommunicationManager : Component, IHabboCommunicationManager, IConnectionStateListener
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::DEFAULT_CONNECTION_ATTEMPTS
    private const int DEFAULT_CONNECTION_ATTEMPTS = 2;

    private readonly Timer _nextPortTimer;
    private ICoreCommunicationManager? _communication;
    private IHabboConfigurationManager? _configurationManager;
    private readonly IMessageConfiguration _messages = new HabboMessages();
    private string _host = string.Empty;
    private readonly Array<int> _ports = [];
    private bool var_1265 = true;
    private int _portIndex = -1;
    private int var_140 = 1;
    private string var_34 = string.Empty;
    private bool var_1277;
    private bool var_1125;
    private readonly Array<int> _a4 = [65191, 65178, 65178, 65177, 65185];
    private int var_1263;
    private bool _requiresInitialRetryAttempt = true;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::HabboCommunicationManager
    public HabboCommunicationManager(IContext param1, uint param2 = 0, object? param3 = null) : base(param1, param2, param3)
    {
        _nextPortTimer = new Timer(100)
        {
            AutoReset = false,
            Enabled = false,
        };
        _nextPortTimer.Elapsed += OnTryNextPort;

        RegisterInterface(new IIDHabboCommunicationManager(), this);

        param1.events.AddEventListener("unload", Unloading);
    }

    // For local demo scripts that construct the manager directly.
    public HabboCommunicationManager() : this(new ComponentContext()) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::dependencies
    protected override IList<ComponentDependency> dependencies =>
        base.dependencies.Concat(
                [
                    new ComponentDependency(new IIDCoreCommunicationManager(),
                        param1 => _communication = param1 as ICoreCommunicationManager),
                    new ComponentDependency(
                        new IIDHabboConfigurationManager(), param1 => _configurationManager = param1 as IHabboConfigurationManager,
                        false, [new DependencyEventListener("complete", OnConfigurationComplete)]
                    ),
                ]
            )
            .ToList();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::onAuthenticated
    private void OnAuthenticated(object? param1)
    {
        _ = param1;
        connection?.IsAuthenticated();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::onConfigurationComplete
    private void OnConfigurationComplete(object? param1)
    {
        _ = param1;
        connection?.IsConfigured();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::disconnect
    public void Disconnect()
    {
        connection?.Close();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::closeConnection
    public void CloseConnection()
    {
        Disconnect();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::connection
    public IConnection? connection { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::mode
    public int mode
    {
        get => 0;
        set => var_1263 = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::port
    public int port
    {
        get
        {
            if (_ports.Count == 0 || _portIndex < 0 || _portIndex >= _ports.Count)
            {
                return 0;
            }

            return _ports[_portIndex];
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::suggestedLoginActions
    public Array suggestedLoginActions { get; set; } = new();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::tcpNoDelay
    public bool tcpNoDelay
    {
        set => var_1265 = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::initComponent
    protected override void InitComponent()
    {
        try
        {
            context.events.AddEventListener("HABBO_CONNECTION_EVENT_AUTHENTICATED", OnAuthenticated);

            connection = _communication?.CreateConnection(this);

            if (connection == null)
            {
                context.Error("Error creating connection: CoreCommunicationManager dependency is null", true, 30);

                return;
            }

            connection.RegisterMessageClasses(_messages);
            connection.AddListener("ioError", OnIoError);
            connection.AddListener("securityError", OnSecurityError);
            connection.AddListener("connect", OnConnect);

            UpdateHostParameters();

            if (_configurationManager?.IsInitialized() == true)
            {
                connection.IsConfigured();
            }

            if (var_1125)
            {
                NextPort();
            }
        }
        catch (Exception e)
        {
            Logger.Error("[HabboCommunicationManager] InitComponent failed", e);

            context.Error("Error creating connection: " + e.Message, false, 30, e);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        context.events.RemoveEventListener("HABBO_CONNECTION_EVENT_AUTHENTICATED", OnAuthenticated);
        context.events.RemoveEventListener("unload", Unloading);

        _nextPortTimer.Stop();

        _nextPortTimer.Elapsed -= OnTryNextPort;

        _nextPortTimer.Dispose();

        if (connection != null)
        {
            connection.Dispose();
            connection = null;
        }

        _configurationManager = null;
        _communication = null;

        base.Dispose();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::updateHostParameters
    public void UpdateHostParameters()
    {
        string host = GetProperty("connection.info.host");

        if (string.IsNullOrEmpty(host))
        {
            context.Error("connection.info.host", true, 30);

            return;
        }

        string ports = GetProperty("connection.info.port");

        if (string.IsNullOrEmpty(ports))
        {
            context.Error("connection.info.host", true, 30);

            return;
        }

        _ports.Clear();

        foreach (string item in ports.Split(','))
        {
            if (int.TryParse(item.Replace(" ", string.Empty), out int parsed))
            {
                _ports.Add(parsed);
            }
        }

        _host = host;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::renewSocket
    public void RenewSocket()
    {
        var_140 = 1;
        _requiresInitialRetryAttempt = true;

        connection?.CreateSocket();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::initConnection
    public void InitConnection(string param1)
    {
        if (!string.Equals(param1, "habbo", StringComparison.Ordinal))
        {
            return;
        }

        if (connection == null)
        {
            context.Error("Tried to connect to proxy but connection was null", true, 30);

            return;
        }

        var_1125 = true;

        if (allRequiredDependenciesInjected)
        {
            NextPort();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::addHabboConnectionMessageEvent
    public IMessageEvent AddHabboConnectionMessageEvent(IMessageEvent param1)
    {
        connection?.AddMessageEvent(param1);

        return param1;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::removeHabboConnectionMessageEvent
    public void RemoveHabboConnectionMessageEvent(IMessageEvent param1)
    {
        connection?.RemoveMessageEvent(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::connectionInit
    public void ConnectionInit(string param1, int param2)
    {
        ErrorReportStorage.SetParameter("host", param1);
        ErrorReportStorage.SetParameter("port", param2.ToString());
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::messageReceived
    public void MessageReceived(string param1)
    {
        ErrorReportStorage.SetParameter("rece_msg_time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

        if (var_34.Length > 0)
        {
            var_34 += ",R:" + param1;
        }
        else
        {
            var_34 = "R:" + param1;
        }

        if (var_34.Length > 150)
        {
            var_34 = var_34[^150..];
        }

        if (PacketLogger.Enabled && int.TryParse(param1, out int id))
        {
            if (_messages.events.TryGetValue(id, out System.Type? t))
                PacketLogger.LogIncoming(id, t.Name);
            else
                PacketLogger.LogUnknownIncoming(id);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::messageSent
    public void MessageSent(string param1)
    {
        ErrorReportStorage.SetParameter("sent_msg_time", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());

        if (var_34.Length > 0)
        {
            var_34 += ",S:" + param1;
        }
        else
        {
            var_34 = "S:" + param1;
        }

        if (var_34.Length > 150)
        {
            var_34 = var_34[^150..];
        }

        if (PacketLogger.Enabled && int.TryParse(param1, out int id))
        {
            string name = _messages.composers.TryGetValue(id, out System.Type? t) ? t.Name : "Unknown";
            PacketLogger.LogOutgoing(id, name);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::messageParseError
    public void MessageParseError(IMessageDataWrapper param1)
    {
        ErrorReportStorage.SetParameter("sent_msg_data", param1.ToString() ?? string.Empty);
        ErrorReportStorage.AddDebugData("MESSAGE_QUEUE", var_34);

        if (PacketLogger.Enabled)
        {
            int id = param1.GetId();
            string name = _messages.events.TryGetValue(id, out System.Type? t) ? t.Name : "Unknown";
            PacketLogger.LogParseError(id, name);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::setMessageQueueErrorDebugData
    public void SetMessageQueueErrorDebugData()
    {
        ErrorReportStorage.AddDebugData("MESSAGE_QUEUE", var_34);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::initializeEncryption
    public IEncryption InitializeEncryption()
    {
        return new ArcFour();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::initializeKeyExchange
    public IKeyExchange InitializeKeyExchange(object? param1, object? param2)
    {
        return new DiffieHellman(param1, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::nextPort
    private void NextPort()
    {
        if (connection == null)
        {
            return;
        }

        if (connection.connected)
        {
            return;
        }

        _portIndex++;

        if (_portIndex >= _ports.Count)
        {
            ErrorReportStorage.AddDebugData("ConnectionRetry", "Connection attempt " + var_140);
            var_140++;

            int retryLimit = DEFAULT_CONNECTION_ATTEMPTS;
            if (_ports.Count == 1)
            {
                retryLimit++;
            }

            if (var_140 > retryLimit)
            {
                if (var_1277)
                {
                    return;
                }

                var_1277 = true;

                context.Error("Connection failed to host and ports", true, 30);

                return;
            }

            _portIndex = 0;
        }

        connection.timeout = var_140 * 10000;
        connection.Init(_host, (uint)_ports[_portIndex], var_1265);

        if (!_requiresInitialRetryAttempt)
        {
            return;
        }

        _portIndex--;
        _requiresInitialRetryAttempt = false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::tryNextPort
    private void TryNextPort()
    {
        if (!_nextPortTimer.Enabled)
        {
            _nextPortTimer.Start();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::unloading
    private void Unloading(object? param1)
    {
        _ = param1;

        connection?.Send(new DisconnectMessageComposer());
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::onIOError
    private void OnIoError(Dictionary param1)
    {
        ErrorReportStorage.AddDebugData(
            "Communication IO Error",
            "IOError " + param1["type"] + " on connect: " + param1["text"] + ". Port was " + port
        );

        TryNextPort();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::onConnect
    private void OnConnect(Dictionary param1)
    {
        _ = param1;

        ErrorReportStorage.AddDebugData("Connection", "Connected with " + var_140 + " attempts");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::onTryNextPort
    private void OnTryNextPort(object? param1, ElapsedEventArgs param2)
    {
        _ = param1;
        _ = param2;

        NextPort();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/HabboCommunicationManager.as::onSecurityError
    private void OnSecurityError(Dictionary param1)
    {
        ErrorReportStorage.AddDebugData("Communication Security Error",
            "SecurityError on connect: " + param1["text"] + ". Port was " + port);

        TryNextPort();
    }
}
