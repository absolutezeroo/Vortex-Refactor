// @see com.sulake.habbo.session.events.UserNameUpdateEvent

namespace Vortex.Habbo.Session.Events;

/// @see com.sulake.habbo.session.events.UserNameUpdateEvent
public class UserNameUpdateEvent
{
    public const string USER_NAME_UPDATE = "UserNameUpdateEvent";

    public UserNameUpdateEvent(int userId, string name)
    {
        UserId = userId;
        Name = name;
        Type = USER_NAME_UPDATE;
    }

    public string Type { get; }
    public int UserId { get; }
    public string Name { get; }
}
