using Vortex.Room.Messages;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Updates avatar body and head direction without movement.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarDirectionUpdateMessage
public class RoomObjectAvatarDirectionUpdateMessage(IVector3d? location, IVector3d? direction, int dirHead) : RoomObjectUpdateMessage(location, direction)
{
    public int DirHead => dirHead;
}
