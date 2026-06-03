// @see com.sulake.habbo.session.events.SessionDataPreferencesEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.SessionDataPreferencesEvent
public class SessionDataPreferencesEvent
{
    public const string SESSION_DATA_PREFERENCES = "SessionDataPreferencesEvent";

    public SessionDataPreferencesEvent(bool cameraFollowDisabled, bool friendBarOpen, bool roomToolsOpen)
    {
        CameraFollowDisabled = cameraFollowDisabled;
        FriendBarOpen = friendBarOpen;
        RoomToolsOpen = roomToolsOpen;
        Type = SESSION_DATA_PREFERENCES;
    }

    public string Type { get; }
    public bool CameraFollowDisabled { get; }
    public bool FriendBarOpen { get; }
    public bool RoomToolsOpen { get; }
}
