// @see com.sulake.habbo.session.events.RoomSessionConfirmPetBreedingResultEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionConfirmPetBreedingResultEvent
public class RoomSessionConfirmPetBreedingResultEvent : RoomSessionEvent
{
    public const string CONFIRM_PET_BREEDING_RESULT = "RSPFUE_CONFIRM_PET_BREEDING_RESULT";

    /// @see RoomSessionConfirmPetBreedingResultEvent.as::RoomSessionConfirmPetBreedingResultEvent
    public RoomSessionConfirmPetBreedingResultEvent(IRoomSession session, int breedingNestStuffId,
        int result, bool openLandingPage = false)
        : base(CONFIRM_PET_BREEDING_RESULT, session, openLandingPage)
    {
        this.breedingNestStuffId = breedingNestStuffId;
        this.result = result;
    }

    /// @see RoomSessionConfirmPetBreedingResultEvent.as::get breedingNestStuffId
    public int breedingNestStuffId { get; }

    /// @see RoomSessionConfirmPetBreedingResultEvent.as::get result
    public int result { get; }
}
