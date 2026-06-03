namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineDragWithMouseEvent
public class RoomEngineDragWithMouseEvent(string type, int roomId) : RoomEngineEvent(type, roomId)
{
    public const string DRAG_START = "REDWME_DRAG_START";
    public const string DRAG_END = "REDWME_DRAG_END";
}
