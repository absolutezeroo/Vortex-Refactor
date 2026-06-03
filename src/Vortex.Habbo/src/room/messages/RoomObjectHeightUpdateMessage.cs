using Vortex.Room.Messages;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Object height change (e.g., stacking height).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectHeightUpdateMessage
public class RoomObjectHeightUpdateMessage(IVector3d? location, IVector3d? direction, double height) : RoomObjectUpdateMessage(location, direction)
{
    public double Height => height;
}
