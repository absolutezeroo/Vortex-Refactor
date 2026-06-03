using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Encryption;
using Vortex.Core.Communication.Handshake;
using Vortex.Core.Communication.Messages;
using Vortex.Core.Runtime;

using Array = Godot.Collections.Array;

namespace Vortex.Habbo.Communication;

/// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as
public interface IHabboCommunicationManager : IUnknown
{
    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::connection
    IConnection? connection { get; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::mode
    int mode { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::suggestedLoginActions
    Array suggestedLoginActions { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::tcpNoDelay
    bool tcpNoDelay { set; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::port
    int port { get; }

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::disconnect
    void Disconnect();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::closeConnection
    void CloseConnection();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::updateHostParameters
    void UpdateHostParameters();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::initConnection
    void InitConnection(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::renewSocket
    void RenewSocket();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::addHabboConnectionMessageEvent
    IMessageEvent AddHabboConnectionMessageEvent(IMessageEvent param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::removeHabboConnectionMessageEvent
    void RemoveHabboConnectionMessageEvent(IMessageEvent param1);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::initializeEncryption
    IEncryption InitializeEncryption();

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::initializeKeyExchange
    IKeyExchange InitializeKeyExchange(object? param1, object? param2);

    /// @see WIN63-202407091256-704579380-Source-main/habbo/communication/IHabboCommunicationManager.as::setMessageQueueErrorDebugData
    void SetMessageQueueErrorDebugData();
}
