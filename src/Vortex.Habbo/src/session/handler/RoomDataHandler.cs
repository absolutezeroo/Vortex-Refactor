// @see com.sulake.habbo.session.handler.RoomDataHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Data;
using Vortex.Habbo.Communication.Messages.Parser.Room.Data;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.RoomDataHandler
public class RoomDataHandler : BaseHandler
{
    /// @see RoomDataHandler.as::RoomDataHandler
    public RoomDataHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new GetGuestRoomResultEvent(OnRoomResult));
    }

    /// @see RoomDataHandler.as::onRoomResult
    private void OnRoomResult(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as GetGuestRoomResultMessageEventParser;
        if (parser == null)
        {
            return;
        }

        if (parser.RoomForward)
        {
            return;
        }

        var session = listener?.GetSession(parser.RoomId);
        if (session == null)
        {
            return;
        }

        session.tradeMode = parser.TradeMode;
        session.isGuildRoom = parser.IsGuildRoom;
        session.doorMode = parser.DoorMode;
        session.arePetsAllowed = parser.ArePetsAllowed;
        session.roomModerationSettings = parser.ModerationSettings;
        listener?.events?.DispatchEvent(new RoomSessionPropertyUpdateEvent(RoomSessionPropertyUpdateEvent.ALLOW_PETS, session));
        listener?.events?.DispatchEvent(new RoomSessionEvent(RoomSessionEvent.SESSION_ROOM_DATA, session));
    }
}
