using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Model key-value number update for object state.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectModelDataUpdateMessage
public class RoomObjectModelDataUpdateMessage(string numberKey, int numberValue) : RoomObjectUpdateMessage(null, null)
{
    public string NumberKey => numberKey;
    public int NumberValue => numberValue;
}
