// @see com.sulake.habbo.session.events.RoomSessionNestBreedingSuccessEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionNestBreedingSuccessEvent
public class RoomSessionNestBreedingSuccessEvent : RoomSessionEvent
{
    public const string NEST_BREEDING_SUCCESS = "RSPFUE_NEST_BREEDING_SUCCESS";

    /// @see RoomSessionNestBreedingSuccessEvent.as::RoomSessionNestBreedingSuccessEvent
    public RoomSessionNestBreedingSuccessEvent(IRoomSession session, int petId, int rarityCategory,
        bool openLandingPage = false)
        : base(NEST_BREEDING_SUCCESS, session, openLandingPage)
    {
        this.petId = petId;
        this.rarityCategory = rarityCategory;
    }

    /// @see RoomSessionNestBreedingSuccessEvent.as::get petId
    public int petId { get; }

    /// @see RoomSessionNestBreedingSuccessEvent.as::get rarityCategory
    public int rarityCategory { get; }
}
