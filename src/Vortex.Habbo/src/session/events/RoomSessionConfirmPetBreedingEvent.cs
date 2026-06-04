// @see com.sulake.habbo.session.events.RoomSessionConfirmPetBreedingEvent

using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionConfirmPetBreedingEvent
public class RoomSessionConfirmPetBreedingEvent : RoomSessionEvent
{
    public const string CONFIRM_PET_BREEDING = "RSPFUE_CONFIRM_PET_BREEDING";

    /// @see RoomSessionConfirmPetBreedingEvent.as::RoomSessionConfirmPetBreedingEvent
    public RoomSessionConfirmPetBreedingEvent(IRoomSession session, int nestId, BreedingPetInfo pet1,
        BreedingPetInfo pet2, IReadOnlyList<int> rarityCategories, int resultPetTypeId,
        bool openLandingPage = false)
        : base(CONFIRM_PET_BREEDING, session, openLandingPage)
    {
        this.nestId = nestId;
        this.pet1 = pet1;
        this.pet2 = pet2;
        this.rarityCategories = rarityCategories;
        this.resultPetTypeId = resultPetTypeId;
    }

    /// @see RoomSessionConfirmPetBreedingEvent.as::get nestId
    public int nestId { get; }

    /// @see RoomSessionConfirmPetBreedingEvent.as::get pet1
    public BreedingPetInfo pet1 { get; }

    /// @see RoomSessionConfirmPetBreedingEvent.as::get pet2
    public BreedingPetInfo pet2 { get; }

    /// @see RoomSessionConfirmPetBreedingEvent.as::get rarityCategories (AS3 Array → IReadOnlyList<int>)
    public IReadOnlyList<int> rarityCategories { get; }

    /// @see RoomSessionConfirmPetBreedingEvent.as::get resultPetTypeId
    public int resultPetTypeId { get; }
}
