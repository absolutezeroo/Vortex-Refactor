namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectDataRequestEvent
public class RoomObjectDataRequestEvent(string type, IRoomObject? obj) : RoomObjectEvent(type, obj)
{
    public const string CURRENT_USER_ID = "RODRE_CURRENT_USER_ID";
    public const string URL_PREFIX = "RODRE_URL_PREFIX";
}
