// @see com.sulake.habbo.session.events.RoomSessionPetInfoUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPetInfoUpdateEvent
public class RoomSessionPetInfoUpdateEvent : RoomSessionEvent
{
    public const string PET_INFO = "RSPIUE_PET_INFO";

    /// @see RoomSessionPetInfoUpdateEvent.as::RoomSessionPetInfoUpdateEvent
    public RoomSessionPetInfoUpdateEvent(IRoomSession session, IPetInfo petInfo, bool openLandingPage = false)
        : base(PET_INFO, session, openLandingPage)
    {
        this.petInfo = petInfo;
    }

    /// @see RoomSessionPetInfoUpdateEvent.as::get petInfo
    public IPetInfo petInfo { get; }
}
