namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar using a floor item (sitting on chair, using machine, etc.).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarUseObjectUpdateMessage
public class RoomObjectAvatarUseObjectUpdateMessage(int itemType) : RoomObjectUpdateStateMessage
{
    public int ItemType => itemType;
}
