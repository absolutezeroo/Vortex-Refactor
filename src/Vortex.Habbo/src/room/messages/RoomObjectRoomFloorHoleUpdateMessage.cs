using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Floor hole add/remove for pool, trapdoor, and other floor cutouts.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomFloorHoleUpdateMessage
public class RoomObjectRoomFloorHoleUpdateMessage
(
    string type,
    int id,
    int x = 0,
    int y = 0,
    int width = 0,
    int height = 0,
    bool invert = false
)
    : RoomObjectUpdateMessage(null, null)
{
    public const string ADD_HOLE = "RORPFHUM_ADD";
    public const string REMOVE_HOLE = "RORPFHUM_REMOVE";

    public string Type => type;
    public int Id => id;
    public int X => x;
    public int Y => y;
    public int Width => width;
    public int Height => height;
    public bool Invert => invert;
}
