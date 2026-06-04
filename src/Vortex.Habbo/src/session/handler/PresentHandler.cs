// @see com.sulake.habbo.session.handler.PresentHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.PresentHandler
public class PresentHandler : BaseHandler
{
    /// @see PresentHandler.as::PresentHandler
    public PresentHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new PresentOpenedMessageEvent(OnPresentOpened));
    }

    /// @see PresentHandler.as::onPresentOpened
    private void OnPresentOpened(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as PresentOpenedMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        if (listener?.events == null)
        {
            return;
        }

        listener.events.DispatchEvent(new RoomSessionPresentEvent(
            RoomSessionPresentEvent.ROOM_SESSION_PRESENT_OPENED, session,
            parser.ClassId, parser.ItemType, parser.ProductCode,
            parser.PlacedItemId, parser.PlacedItemType, parser.PlacedInRoom, parser.PetFigureString));
    }
}
