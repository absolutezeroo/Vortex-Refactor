// @see com.sulake.habbo.session.handler.RoomPermissionsHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Permissions;
using Vortex.Habbo.Communication.Messages.Parser.Room.Permissions;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.RoomPermissionsHandler
public class RoomPermissionsHandler : BaseHandler
{
    /// @see RoomPermissionsHandler.as::RoomPermissionsHandler
    public RoomPermissionsHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new YouAreControllerMessageEvent(OnYouAreController));
        connection.AddMessageEvent(new YouAreNotControllerMessageEvent(OnYouAreNotController));
        connection.AddMessageEvent(new YouAreOwnerMessageEvent(OnYouAreOwner));
    }

    /// @see RoomPermissionsHandler.as::onYouAreController
    private void OnYouAreController(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as YouAreControllerMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(parser.FlatId);
        if (session == null)
        {
            return;
        }

        session.roomControllerLevel = parser.RoomControllerLevel;
    }

    /// @see RoomPermissionsHandler.as::onYouAreNotController
    private void OnYouAreNotController(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as YouAreNotControllerMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(parser.FlatId);
        if (session == null)
        {
            return;
        }

        session.roomControllerLevel = 0;
    }

    /// @see RoomPermissionsHandler.as::onYouAreOwner
    private void OnYouAreOwner(IMessageEvent ev)
    {
        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        session.isRoomOwner = true;
    }
}
