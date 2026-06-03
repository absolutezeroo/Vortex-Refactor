using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Room wall/floor/landscape texture or style update.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectRoomUpdateMessage
public class RoomObjectRoomUpdateMessage(string type, string value) : RoomObjectUpdateMessage(null, null)
{
    public const string ROOM_WALL_UPDATE = "RORUM_ROOM_WALL_UPDATE";
    public const string ROOM_FLOOR_UPDATE = "RORUM_ROOM_FLOOR_UPDATE";
    public const string ROOM_LANDSCAPE_UPDATE = "RORUM_ROOM_LANDSCAPE_UPDATE";

    public string Type => type;
    public string Value => value;
}
