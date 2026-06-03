using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Wall item data string update.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectItemDataUpdateMessage
public class RoomObjectItemDataUpdateMessage(string itemData) : RoomObjectUpdateMessage(null, null)
{
    public string ItemData => itemData;
}
