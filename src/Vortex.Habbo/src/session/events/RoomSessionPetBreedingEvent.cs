// @see com.sulake.habbo.session.events.RoomSessionPetBreedingEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetBreedingEvent
public class RoomSessionPetBreedingEvent : RoomSessionEvent
{
    public const string PET_BREEDING = "RSPFUE_PET_BREEDING";

    /// @see RoomSessionPetBreedingEvent.as::RoomSessionPetBreedingEvent
    public RoomSessionPetBreedingEvent(IRoomSession session, int state, int ownPetId, int otherPetId,
        bool openLandingPage = false)
        : base(PET_BREEDING, session, openLandingPage)
    {
        this.state = state;
        this.ownPetId = ownPetId;
        this.otherPetId = otherPetId;
    }

    /// @see RoomSessionPetBreedingEvent.as::get state
    public int state { get; }

    /// @see RoomSessionPetBreedingEvent.as::get ownPetId
    public int ownPetId { get; }

    /// @see RoomSessionPetBreedingEvent.as::get otherPetId
    public int otherPetId { get; }
}
