// @see com.sulake.habbo.session.events.RoomSessionPropertyUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionPropertyUpdateEvent
public class RoomSessionPropertyUpdateEvent : RoomSessionEvent
{
    public const string ALLOW_PETS = "RSDUE_ALLOW_PETS";

    /// @see RoomSessionPropertyUpdateEvent.as::RoomSessionPropertyUpdateEvent
    public RoomSessionPropertyUpdateEvent(string type, IRoomSession session, bool openLandingPage = false)
        : base(type, session, openLandingPage)
    {
    }
}
