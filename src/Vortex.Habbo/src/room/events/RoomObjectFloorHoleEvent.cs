namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectFloorHoleEvent
public class RoomObjectFloorHoleEvent(string type, IRoomObject? obj) : RoomObjectEvent(type, obj)
{
    public const string ADD_HOLE = "ROFHO_ADD_HOLE";
    public const string REMOVE_HOLE = "ROFHO_REMOVE_HOLE";
}
