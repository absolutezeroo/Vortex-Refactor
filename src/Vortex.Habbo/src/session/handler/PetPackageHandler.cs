// @see com.sulake.habbo.session.handler.PetPackageHandler

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;
using Vortex.Habbo.Session.Events;

namespace Vortex.Habbo.Session.Handler;

/// @see com.sulake.habbo.session.handler.PetPackageHandler
public class PetPackageHandler : BaseHandler
{
    /// @see PetPackageHandler.as::PetPackageHandler
    public PetPackageHandler(IConnection? connection, IRoomHandlerListener listener)
        : base(connection, listener)
    {
        if (connection == null)
        {
            return;
        }

        connection.AddMessageEvent(new OpenPetPackageRequestedMessageEvent(OnOpenPetPackageRequested));
        connection.AddMessageEvent(new OpenPetPackageResultMessageEvent(OnOpenPetPackageResult));
    }

    /// @see PetPackageHandler.as::onOpenPetPackageRequested
    private void OnOpenPetPackageRequested(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as OpenPetPackageRequestedMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetPackageEvent(
            RoomSessionPetPackageEvent.ROOM_SESSION_OPEN_PET_PACKAGE_REQUESTED,
            session, parser.ObjectId, parser.FigureData, 0, null));
    }

    /// @see PetPackageHandler.as::onOpenPetPackageResult
    private void OnOpenPetPackageResult(IMessageEvent ev)
    {
        var parser = (ev as MessageEvent)?.parser as OpenPetPackageResultMessageEventParser;
        if (parser == null)
        {
            return;
        }

        var session = listener?.GetSession(currentRoomId);
        if (session == null)
        {
            return;
        }

        listener?.events?.DispatchEvent(new RoomSessionPetPackageEvent(
            RoomSessionPetPackageEvent.ROOM_SESSION_OPEN_PET_PACKAGE_RESULT,
            session, parser.ObjectId, null, parser.NameValidationStatus, parser.NameValidationInfo));
    }
}
