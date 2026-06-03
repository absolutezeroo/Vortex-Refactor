using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Object visibility toggle (enabled/disabled).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectVisibilityUpdateMessage
public class RoomObjectVisibilityUpdateMessage(string type) : RoomObjectUpdateMessage(null, null)
{
    public const string ENABLED = "ROVUM_ENABLED";
    public const string DISABLED = "ROVUM_DISABLED";

    public string Type => type;
}
