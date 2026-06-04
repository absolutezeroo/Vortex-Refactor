using System;
using System.Numerics;

using Godot;
using Godot.Collections;

using Vortex.Core;
using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Handshake;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Availability;
using Vortex.Habbo.Communication.Messages.Incoming.Error;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Outgoing.Handshake;
using Vortex.Habbo.Communication.Messages.Outgoing.Tracking;
using Vortex.Habbo.Communication.Messages.Parser.Availability;
using Vortex.Habbo.Communication.Messages.Parser.Error;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Demo;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as
public class IncomingMessages
{
    private readonly List<IMessageEvent> _messageEvents = [];
    private IHabboCommunicationManager? _communication;
    private string _privateKey = string.Empty;
    private RSAKey? _rsa;
    private HabboCommunicationDemo? var_1660;
    private IKeyExchange? var_2512;
    private bool var_2764;
    private bool var_2788;

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::IncomingMessages
    public IncomingMessages(HabboCommunicationDemo param1, IHabboCommunicationManager param2)
    {
        var_1660 = param1;
        _communication = param2;

        IConnection? connection = _communication.connection ?? throw new Exception("Connection is required to initialize!");

        connection.AddListener("connect", _ => OnConnectionEstablished());
        connection.AddListener("close", OnConnectionDisconnected);

        AddHabboConnectionMessageEvent(new LoginFailedHotelClosedMessageEvent(OnLoginFailedHotelClosed));
        AddHabboConnectionMessageEvent(new DisconnectReasonEvent(OnDisconnectReason));
        AddHabboConnectionMessageEvent(new MaintenanceStatusMessageEvent(OnMaintenance));
        AddHabboConnectionMessageEvent(new GenericErrorEvent(OnGenericError));
        AddHabboConnectionMessageEvent(new InitDiffieHandshakeEvent(OnInitDiffieHandshake));
        AddHabboConnectionMessageEvent(new UniqueMachineIdEvent(OnUniqueMachineId));
        AddHabboConnectionMessageEvent(new CompleteDiffieHandshakeEvent(OnCompleteDiffieHandshake));
        AddHabboConnectionMessageEvent(new ErrorReportEvent(OnErrorReport));
        AddHabboConnectionMessageEvent(new IdentityAccountsEvent(OnIdentityAccounts));
        AddHabboConnectionMessageEvent(new PingMessageEvent(OnPing));
        AddHabboConnectionMessageEvent(new AuthenticationOkMessageEvent(OnAuthenticationOk));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::dispose
    public void Dispose()
    {
        if (_communication != null)
        {
            foreach (IMessageEvent messageEvent in _messageEvents)
            {
                _communication.RemoveHabboConnectionMessageEvent(messageEvent);
            }
            _messageEvents.Clear();
        }

        _rsa?.Dispose();
        _rsa = null;
        var_1660 = null;
        _communication = null;
        var_2512 = null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::addHabboConnectionMessageEvent
    private void AddHabboConnectionMessageEvent(IMessageEvent param1)
    {
        _communication?.AddHabboConnectionMessageEvent(param1);
        _messageEvents.Add(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onInitDiffieHandshake
    private void OnInitDiffieHandshake(IMessageEvent param1)
    {
        IConnection? connection = param1.connection;
        if (connection == null || _communication == null)
        {
            return;
        }

        InitDiffieHandshakeEvent evt = (InitDiffieHandshakeEvent)param1;

        _rsa = RSAKey.ParsePublicKey(
            "86851DD364D5C5CECE3C883171CC6DDC5760779B992482BD1E20DD296888DF91B33B936A7B93F06D29E8870F703A216257DEC7C81DE0058FEA4CC5116F75E6EFC4E9113513E45357DC3FD43D4EFAB5963EF178B78BD61E81A14C603B24C8BCCE0A12230B320045498EDC29282FF0603BC7B7DAE8FC1B05B52B2F301A9DC783B7",
            "3"
        );

        string decryptedPrime = _rsa.DecryptString(evt.encryptedPrime);
        string decryptedGenerator = _rsa.DecryptString(evt.encryptedGenerator);

        BigInteger prime = BigInteger.Parse(decryptedPrime);
        BigInteger generator = BigInteger.Parse(decryptedGenerator);

        if (prime <= 2 || generator >= prime || prime == generator)
        {
            CoreEnvironment.Crash("Invalid DH prime and generator", 29);
            return;
        }

        var_2512 = _communication.InitializeKeyExchange(prime, generator);

        string? bestPublicKey = null;
        string? lastCandidate = null;
        int retries = 10;

        while (retries > 0)
        {
            lastCandidate = GenerateRandomHexString(30);
            var_2512.Init(lastCandidate);

            string publicKey = var_2512.GetPublicKey(10);

            if (publicKey.Length >= 64)
            {
                bestPublicKey = publicKey;
                _privateKey = lastCandidate;
                break;
            }

            if (bestPublicKey == null || publicKey.Length > bestPublicKey.Length)
            {
                bestPublicKey = publicKey;
                _privateKey = lastCandidate;
            }

            retries--;
        }

        if (lastCandidate != _privateKey)
        {
            var_2512.Init(_privateKey);
        }

        string encryptedPublicKey = _rsa.EncryptString(bestPublicKey ?? string.Empty);

        connection.SendUnencrypted(new CompleteDiffieHandshakeMessageComposer(encryptedPublicKey));
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onCompleteDiffieHandshake
    private void OnCompleteDiffieHandshake(IMessageEvent param1)
    {
        IConnection? connection = param1.connection;
        if (connection == null || _communication == null || var_2512 == null || var_1660 == null)
        {
            return;
        }

        CompleteDiffieHandshakeEvent evt = (CompleteDiffieHandshakeEvent)param1;

        string decryptedPublicKey = _rsa!.DecryptString(evt.encryptedPublicKey);
        _rsa.Dispose();
        _rsa = null;

        var_2512.GenerateSharedKey(decryptedPublicKey, 10);

        string sharedKeyHex = var_2512.GetSharedKey(16).ToUpperInvariant();

        if (!var_2512.IsValidServerPublicKey())
        {
            return;
        }

        byte[] keyBytes = CryptoTools.HexStringToByteArray(sharedKeyHex);
        IEncryption c2S = _communication.InitializeEncryption();
        c2S.Init(keyBytes);

        IEncryption? s2C = null;

        if (evt.serverClientEncryption)
        {
            s2C = _communication.InitializeEncryption();
            s2C.Init(keyBytes);
        }

        connection.SetEncryption(c2S, s2C);

        var_2764 = false;
        var_1660.DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_HANDSHAKED");
        var_1660.SendConnectionParameters(connection);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onAuthenticationOK
    private void OnAuthenticationOk(IMessageEvent param1)
    {
        if (var_1660 == null || _communication == null)
        {
            return;
        }

        AuthenticationOkMessageEvent evt = (AuthenticationOkMessageEvent)param1;
        IConnection? connection = param1.connection;

        if (connection == null)
        {
            return;
        }

        var_1660.DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_AUTHENTICATED");

        connection.Send(new InfoRetrieveMessageComposer());
        connection.Send(new EventLogMessageComposer("Login", "socket", "client.auth_ok"));

        _communication.suggestedLoginActions = evt.suggestedLoginActions;

        var_1660.LoginOk();
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onLoginFailedHotelClosed
    private void OnLoginFailedHotelClosed(IMessageEvent param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        LoginFailedHotelClosedMessageEventParser parser = ((LoginFailedHotelClosedMessageEvent)param1).GetParser();

        var_1660.HandleLoginFailedHotelClosedMessage(parser.openHour, parser.openMinute);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onGenericError
    private void OnGenericError(IMessageEvent param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        GenericErrorEventParser parser = ((GenericErrorEvent)param1).GetParser();

        switch (parser.errorCode)
        {
            case -3:
                HabboCommunicationDemo.Alert("${connection.error.id.title}", "${connection.login.error.-3.desc}");
                break;
            case -400:
                HabboCommunicationDemo.Alert("${connection.error.id.title}", "${connection.login.error.-400.desc}");
                break;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onPing
    private void OnPing(IMessageEvent param1)
    {
        param1.connection?.Send(new PongMessageComposer());
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onUniqueMachineId
    private void OnUniqueMachineId(IMessageEvent param1) { }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onIdentityAccounts
    private void OnIdentityAccounts(IMessageEvent param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        IdentityAccountsEventParser parser = ((IdentityAccountsEvent)param1).GetParser();
        Array<Dictionary> avatars = new();

        foreach (int key in parser.accounts.Keys)
        {
            avatars.Add(
                new Dictionary
                {
                    ["id"] = key,
                    ["name"] = parser.accounts[key],
                    ["uniqueId"] = key.ToString(),
                }
            );
        }

        HabboCommunicationDemo.OnUserList(avatars);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onErrorReport
    private void OnErrorReport(IMessageEvent param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        ErrorReportEventParser parser = ((ErrorReportEvent)param1).GetParser();

        var_1660.HandleErrorMessage(parser.errorCode, parser.messageId);
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onMaintenance
    private void OnMaintenance(IMessageEvent param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        MaintenanceStatusMessageEventParser parser = ((MaintenanceStatusMessageEvent)param1).GetParser();

        var_1660.Disconnected(-2, $"maintenance in {parser.minutesUntilMaintenance}");
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onDisconnectReason
    private void OnDisconnectReason(IMessageEvent param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        DisconnectReasonEvent evt = (DisconnectReasonEvent)param1;

        if (var_2764)
        {
            var_1660.DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_HANDSHAKE_FAIL");
        }

        var_1660.Disconnected(evt.reason, evt.GetReasonName());
        var_2764 = false;
        var_2788 = true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onConnectionEstablished
    private void OnConnectionEstablished()
    {
        if (var_1660 == null || _communication?.connection == null)
        {
            Logger.Warn(
                $"[IncomingMessages] OnConnectionEstablished aborted: demo={var_1660 != null} connection={_communication?.connection != null}"
            );
            return;
        }

        var_1660.DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_ESTABLISHED");

        var_2788 = false;
        var_2764 = true;

        var_1660.DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_HANDSHAKING");

        _communication.connection.SendUnencrypted(new ClientHelloMessageComposer());
        _communication.connection.SendUnencrypted(new InitDiffieHandshakeMessageComposer());
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::onConnectionDisconnected
    private void OnConnectionDisconnected(Dictionary param1)
    {
        if (var_1660 == null)
        {
            return;
        }

        if (HabboCommunicationDemo.isRoomViewerMode)
        {
            return;
        }

        if (var_2764)
        {
            var_1660.DispatchLoginStepEvent("HABBO_CONNECTION_EVENT_HANDSHAKE_FAIL");
        }

        if (param1.TryGetValue("type", out Variant type) && type.AsString() == "close" && !var_2788)
        {
            var_1660.Disconnected(-3, string.Empty);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/demo/IncomingMessages.as::generateRandomHexString
    private static string GenerateRandomHexString(int length = 16)
    {
        string text = string.Empty;

        for (int i = 0;
             i < length;
             i++)
        {
            int value = GD.RandRange(0, 255);

            text += value.ToString("x2");
        }

        return text;
    }
}
