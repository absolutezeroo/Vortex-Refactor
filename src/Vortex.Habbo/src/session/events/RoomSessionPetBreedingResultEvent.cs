// @see com.sulake.habbo.session.events.RoomSessionPetBreedingResultEvent

using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetBreedingResultEvent
public class RoomSessionPetBreedingResultEvent : RoomSessionEvent
{
    public const string PET_BREEDING_RESULT = "RSPFUE_PET_BREEDING_RESULT";

    /// @see RoomSessionPetBreedingResultEvent.as::RoomSessionPetBreedingResultEvent
    public RoomSessionPetBreedingResultEvent(IRoomSession session, PetBreedingResultData resultData,
        PetBreedingResultData otherResultData, bool openLandingPage = false)
        : base(PET_BREEDING_RESULT, session, openLandingPage)
    {
        this.resultData = resultData;
        this.otherResultData = otherResultData;
    }

    /// @see RoomSessionPetBreedingResultEvent.as::get resultData
    public PetBreedingResultData resultData { get; }

    /// @see RoomSessionPetBreedingResultEvent.as::get otherResultData
    public PetBreedingResultData otherResultData { get; }
}
