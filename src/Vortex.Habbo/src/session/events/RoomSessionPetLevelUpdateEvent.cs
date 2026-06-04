// @see com.sulake.habbo.session.events.RoomSessionPetLevelUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetLevelUpdateEvent
public class RoomSessionPetLevelUpdateEvent : RoomSessionEvent
{
    public const string PET_LEVEL_UPDATE = "RSPLUE_PET_LEVEL_UPDATE";

    /// @see RoomSessionPetLevelUpdateEvent.as::RoomSessionPetLevelUpdateEvent
    public RoomSessionPetLevelUpdateEvent(IRoomSession session, int petId, int level,
        bool openLandingPage = false)
        : base(PET_LEVEL_UPDATE, session, openLandingPage)
    {
        this.petId = petId;
        this.level = level;
    }

    /// @see RoomSessionPetLevelUpdateEvent.as::get petId
    public int petId { get; }

    /// @see RoomSessionPetLevelUpdateEvent.as::get level
    public int level { get; }
}
