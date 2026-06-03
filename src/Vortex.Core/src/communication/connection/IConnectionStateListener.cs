using Vortex.Core.Communication.Messages;

namespace Vortex.Core.Communication.Connection;

/// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_26.as
public interface IConnectionStateListener
{
    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_26.as::connectionInit
    void ConnectionInit(string param1, int param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_26.as::messageReceived
    void MessageReceived(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_26.as::messageSent
    void MessageSent(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/connection/class_26.as::messageParseError
    void MessageParseError(IMessageDataWrapper param1);
}
