// @see com.sulake.habbo.session.events.RoomSessionFriendRequestEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionFriendRequestEvent
public class RoomSessionFriendRequestEvent : RoomSessionEvent
{
    public const string FRIEND_REQUEST = "RSFRE_FRIEND_REQUEST";

    /// @see RoomSessionFriendRequestEvent.as::RoomSessionFriendRequestEvent
    public RoomSessionFriendRequestEvent(IRoomSession session, int requestId, int userId, string userName,
        bool openLandingPage = false)
        : base(FRIEND_REQUEST, session, openLandingPage)
    {
        this.requestId = requestId;
        this.userId = userId;
        this.userName = userName;
    }

    /// @see RoomSessionFriendRequestEvent.as::get requestId
    public int requestId { get; }

    /// @see RoomSessionFriendRequestEvent.as::get userId
    public int userId { get; }

    /// @see RoomSessionFriendRequestEvent.as::get userName
    public string userName { get; }
}
