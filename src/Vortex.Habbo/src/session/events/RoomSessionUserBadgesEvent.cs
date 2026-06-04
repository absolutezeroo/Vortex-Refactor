// @see com.sulake.habbo.session.events.RoomSessionUserBadgesEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionUserBadgesEvent
public class RoomSessionUserBadgesEvent : RoomSessionEvent
{
    public const string USER_BADGES = "RSUBE_BADGES";

    /// @see RoomSessionUserBadgesEvent.as::RoomSessionUserBadgesEvent
    public RoomSessionUserBadgesEvent(IRoomSession session, int userId, IReadOnlyList<string> badges)
        : base(USER_BADGES, session)
    {
        this.userId = userId;
        this.badges = badges;
    }

    /// @see RoomSessionUserBadgesEvent.as::get userId
    public int userId { get; }

    /// @see RoomSessionUserBadgesEvent.as::get badges
    public IReadOnlyList<string> badges { get; }
}
