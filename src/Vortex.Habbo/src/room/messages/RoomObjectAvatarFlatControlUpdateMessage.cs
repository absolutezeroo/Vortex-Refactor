namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar flat control (room rights) level update with admin flag.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarFlatControlUpdateMessage
public class RoomObjectAvatarFlatControlUpdateMessage(string rawData) : RoomObjectUpdateStateMessage
{
    public string RawData => rawData;
    public bool IsAdmin { get; }
}
