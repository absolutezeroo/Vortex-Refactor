namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar is carrying or using a hand item (drink, food, etc.).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarCarryObjectUpdateMessage
public class RoomObjectAvatarCarryObjectUpdateMessage(int itemType, string itemName) : RoomObjectUpdateStateMessage
{
    public int ItemType => itemType;
    public string ItemName => itemName;
}
