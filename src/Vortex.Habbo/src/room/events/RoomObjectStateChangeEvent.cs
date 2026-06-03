namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectStateChangeEvent
public class RoomObjectStateChangeEvent(string type, IRoomObject? obj, int param = 0) : RoomObjectEvent(type, obj)
{
    public const string ROOM_OBJECT_STATE_CHANGE = "ROSCE_STATE_CHANGE";
    public const string ROOM_OBJECT_STATE_RANDOM = "ROSCE_STATE_RANDOM";

    public int Param => param;
}
