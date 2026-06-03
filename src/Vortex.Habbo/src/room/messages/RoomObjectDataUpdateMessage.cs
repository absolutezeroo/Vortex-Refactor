using Vortex.Room.Messages;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Furniture state/data update with stuff data payload.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectDataUpdateMessage
public class RoomObjectDataUpdateMessage(int state, IStuffData? data, double extra = double.NaN) : RoomObjectUpdateMessage(null, null)
{
    public int State => state;
    public IStuffData? Data => data;
    public double Extra => extra;
}
