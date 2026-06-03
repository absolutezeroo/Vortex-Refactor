using Vortex.Room.Events;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomToObjectOwnAvatarMoveEvent
public class RoomToObjectOwnAvatarMoveEvent(string type, IVector3d targetLoc) : RoomToObjectEvent(type)
{
    public const string MOVE_TO = "ROAME_MOVE_TO";

    public IVector3d TargetLoc { get; } = targetLoc;
}
