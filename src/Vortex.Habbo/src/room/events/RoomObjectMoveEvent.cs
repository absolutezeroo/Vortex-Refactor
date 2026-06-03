using Vortex.Room.Events;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomObjectMoveEvent
public class RoomObjectMoveEvent(string type, IRoomObject? obj) : RoomObjectEvent(type, obj)
{
    public const string SLIDE_ANIMATION = "ROME_SLIDE_ANIMATION";
    public const string POSITION_CHANGED = "ROME_POSITION_CHANGED";
    public const string OBJECT_REMOVED = "ROME_OBJECT_REMOVED";
}
