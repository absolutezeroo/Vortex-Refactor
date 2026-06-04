// @see com.sulake.habbo.session.events.RoomSessionEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionEvent
public class RoomSessionEvent
{
    public const string RSE_CREATED = "RSE_CREATED";
    public const string RSE_STARTED = "RSE_STARTED";
    public const string RSE_ENDED = "RSE_ENDED";
    public const string SESSION_ROOM_DATA = "RSE_ROOM_DATA";

    public RoomSessionEvent(string type, IRoomSession session, bool openLandingPage = true)
    {
        this.type = type;
        this.session = session;
        this.openLandingPage = openLandingPage;
    }

    /// @see RoomSessionEvent.as::type (used by EventDispatcherWrapper reflection dispatch)
    public string type { get; }

    /// @see RoomSessionEvent.as::get session
    public IRoomSession session { get; }

    /// @see RoomSessionEvent.as::get openLandingPage
    public bool openLandingPage { get; }
}
