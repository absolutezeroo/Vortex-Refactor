using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Wall or floor plane visibility toggle.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomPlaneVisibilityUpdateMessage
public class RoomObjectRoomPlaneVisibilityUpdateMessage(string type, bool visible) : RoomObjectUpdateMessage(null, null)
{
    public const string WALL_VISIBILITY = "RORPVUM_WALL_VISIBILITY";
    public const string FLOOR_VISIBILITY = "RORPVUM_FLOOR_VISIBILITY";

    public string Type => type;
    public bool Visible => visible;
}
