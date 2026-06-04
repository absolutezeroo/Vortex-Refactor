// @see com.sulake.habbo.session.handler.RoomDataHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
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
            return;
        // TODO(as3-port): GetGuestRoomResultEvent not yet ported
    }

    // TODO(as3-port): onRoomResult — requires GetGuestRoomResultEvent + GetGuestRoomResultMessageParser
    // Logic: if roomForward → return; else set session.tradeMode, isGuildRoom, doorMode, arePetsAllowed,
    // roomModerationSettings; dispatch RoomSessionPropertyUpdateEvent + RoomSessionEvent("RSE_ROOM_DATA")
}
