// @see com.sulake.habbo.session.handler.GenericErrorHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.GenericErrorHandler
public class GenericErrorHandler : BaseHandler
{
    /// @see GenericErrorHandler.as::GenericErrorHandler
    public GenericErrorHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
            return;
        connection.AddMessageEvent(new GenericErrorEvent(OnGenericError));
    }

    /// @see GenericErrorHandler.as::onGenericError
    private void OnGenericError(IMessageEvent ev)
    {
        var errorEv = ev as GenericErrorEvent;
        var parser = errorEv?.GetParser();
        if (parser == null)
            return;
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
            return;
        string? errorType = parser.errorCode switch
        {
            4008 => RoomSessionErrorMessageEvent.KICKED_BY_OWNER,
            _ => null,
        };
        if (errorType != null && listener?.events != null)
            listener.events.DispatchEvent(new RoomSessionErrorMessageEvent(errorType, session));
    }
}
