// @see com.sulake.habbo.session.events.RoomSessionPetStatusUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetStatusUpdateEvent
public class RoomSessionPetStatusUpdateEvent : RoomSessionEvent
{
    public const string PET_STATUS_UPDATE = "RSPFUE_PET_STATUS_UPDATE";

    /// @see RoomSessionPetStatusUpdateEvent.as::RoomSessionPetStatusUpdateEvent
    public RoomSessionPetStatusUpdateEvent(IRoomSession session, int petId, bool canBreed,
        bool canHarvest, bool canRevive, bool hasBreedingPermission, bool openLandingPage = false)
        : base(PET_STATUS_UPDATE, session, openLandingPage)
    {
        this.petId = petId;
        this.canBreed = canBreed;
        this.canHarvest = canHarvest;
        this.canRevive = canRevive;
        this.hasBreedingPermission = hasBreedingPermission;
    }

    /// @see RoomSessionPetStatusUpdateEvent.as::get petId
    public int petId { get; }

    /// @see RoomSessionPetStatusUpdateEvent.as::get canBreed
    public bool canBreed { get; }

    /// @see RoomSessionPetStatusUpdateEvent.as::get canHarvest
    public bool canHarvest { get; }

    /// @see RoomSessionPetStatusUpdateEvent.as::get canRevive
    public bool canRevive { get; }

    /// @see RoomSessionPetStatusUpdateEvent.as::get hasBreedingPermission
    public bool hasBreedingPermission { get; }
}
