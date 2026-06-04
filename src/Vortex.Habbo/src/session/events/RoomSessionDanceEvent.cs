// @see com.sulake.habbo.session.events.RoomSessionDanceEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionDanceEvent
public class RoomSessionDanceEvent : RoomSessionEvent
{
    public const string DANCE = "RSDE_DANCE";

    /// @see RoomSessionDanceEvent.as::RoomSessionDanceEvent
    public RoomSessionDanceEvent(IRoomSession session, int userId, int danceStyle,
        bool openLandingPage = false)
        : base(DANCE, session, openLandingPage)
    {
        this.userId = userId;
        this.danceStyle = danceStyle;
    }

    /// @see RoomSessionDanceEvent.as::get userId
    public int userId { get; }

    /// @see RoomSessionDanceEvent.as::get danceStyle
    public int danceStyle { get; }
}
