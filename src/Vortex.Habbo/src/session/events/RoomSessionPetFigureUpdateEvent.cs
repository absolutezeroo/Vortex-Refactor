// @see com.sulake.habbo.session.events.RoomSessionPetFigureUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetFigureUpdateEvent
public class RoomSessionPetFigureUpdateEvent : RoomSessionEvent
{
    public const string PET_FIGURE_UPDATE = "RSPFUE_PET_FIGURE_UPDATE";

    /// @see RoomSessionPetFigureUpdateEvent.as::RoomSessionPetFigureUpdateEvent
    public RoomSessionPetFigureUpdateEvent(IRoomSession session, int petId, string figure, bool openLandingPage = false)
        : base(PET_FIGURE_UPDATE, session, openLandingPage)
    {
        this.petId = petId;
        this.figure = figure;
    }

    /// @see RoomSessionPetFigureUpdateEvent.as::get petId
    public int petId { get; }

    /// @see RoomSessionPetFigureUpdateEvent.as::get figure
    public string figure { get; }
}
