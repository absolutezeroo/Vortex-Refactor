using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Wall or floor thickness property change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomPlanePropertyUpdateMessage
public class RoomObjectRoomPlanePropertyUpdateMessage(string type, double value) : RoomObjectUpdateMessage(null, null)
{
    public const string WALL_THICKNESS = "RORPPUM_WALL_THICKNESS";
    public const string FLOOR_THICKNESS = "RORPVUM_FLOOR_THICKNESS";

    public string Type => type;
    public double Value => value;
}
