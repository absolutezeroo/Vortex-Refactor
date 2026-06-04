// @see com.sulake.habbo.session.events.RoomSessionDoorbellEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.RoomSessionDoorbellEvent
public class RoomSessionDoorbellEvent : RoomSessionEvent
{
    public const string DOORBELL = "RSDE_DOORBELL";
    public const string REJECTED = "RSDE_REJECTED";
    public const string ACCEPTED = "RSDE_ACCEPTED";

    /// @see RoomSessionDoorbellEvent.as::RoomSessionDoorbellEvent
    public RoomSessionDoorbellEvent(string type, IRoomSession session, string userName)
        : base(type, session)
    {
        this.userName = userName;
    }

    /// @see RoomSessionDoorbellEvent.as::get userName
    public string userName { get; }
}
