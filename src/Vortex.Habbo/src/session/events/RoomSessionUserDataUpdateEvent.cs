// @see com.sulake.habbo.session.events.RoomSessionUserDataUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionUserDataUpdateEvent
public class RoomSessionUserDataUpdateEvent : RoomSessionEvent
{
    public const string USER_DATA_UPDATED = "rsudue_user_data_updated";

    /// @see RoomSessionUserDataUpdateEvent.as::RoomSessionUserDataUpdateEvent
    public RoomSessionUserDataUpdateEvent(IRoomSession session, IReadOnlyList<IUserData> addedUsers)
        : base(USER_DATA_UPDATED, session)
    {
        this.addedUsers = addedUsers;
    }

    /// @see RoomSessionUserDataUpdateEvent.as::get addedUsers
    public IReadOnlyList<IUserData> addedUsers { get; }
}
