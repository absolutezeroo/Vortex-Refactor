// @see com.sulake.habbo.session.events.RoomSessionPetPackageEvent

using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetPackageEvent
public class RoomSessionPetPackageEvent : RoomSessionEvent
{
    public const string ROOM_SESSION_OPEN_PET_PACKAGE_REQUESTED = "RSOPPE_OPEN_PET_PACKAGE_REQUESTED";
    public const string ROOM_SESSION_OPEN_PET_PACKAGE_RESULT = "RSOPPE_OPEN_PET_PACKAGE_RESULT";

    /// @see RoomSessionPetPackageEvent.as::RoomSessionPetPackageEvent
    public RoomSessionPetPackageEvent(string type, IRoomSession session, int objectId,
        PetFigureData? figureData, int nameValidationStatus, string? nameValidationInfo,
        bool openLandingPage = false)
        : base(type, session, openLandingPage)
    {
        this.objectId = objectId;
        this.figureData = figureData;
        this.nameValidationStatus = nameValidationStatus;
        this.nameValidationInfo = nameValidationInfo;
    }

    /// @see RoomSessionPetPackageEvent.as::get objectId
    public int objectId { get; }

    /// @see RoomSessionPetPackageEvent.as::get figureData
    public PetFigureData? figureData { get; }

    /// @see RoomSessionPetPackageEvent.as::get nameValidationStatus
    public int nameValidationStatus { get; }

    /// @see RoomSessionPetPackageEvent.as::get nameValidationInfo
    public string? nameValidationInfo { get; }
}
