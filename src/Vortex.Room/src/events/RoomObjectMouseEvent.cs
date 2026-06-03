using Vortex.Room.Object;

namespace Vortex.Room.Events;

/// @see com.sulake.room.events.RoomObjectMouseEvent
public class RoomObjectMouseEvent(string type,
    IRoomObject? obj,
    string eventId,
    bool altKey = false,
    bool ctrlKey = false,
    bool shiftKey = false,
    bool buttonDown = false)
    : RoomObjectEvent(type, obj)
{
    public const string ROOM_OBJECT_MOUSE_CLICK = "ROE_MOUSE_CLICK";
    public const string ROOM_OBJECT_MOUSE_ENTER = "ROE_MOUSE_ENTER";
    public const string ROOM_OBJECT_MOUSE_MOVE = "ROE_MOUSE_MOVE";
    public const string ROOM_OBJECT_MOUSE_LEAVE = "ROE_MOUSE_LEAVE";
    public const string ROOM_OBJECT_MOUSE_DOUBLE_CLICK = "ROE_MOUSE_DOUBLE_CLICK";
    public const string ROOM_OBJECT_MOUSE_DOWN = "ROE_MOUSE_DOWN";

    public string EventId { get; } = eventId;
    public bool AltKey { get; } = altKey;
    public bool CtrlKey { get; } = ctrlKey;
    public bool ShiftKey { get; } = shiftKey;
    public bool ButtonDown { get; } = buttonDown;
    public int LocalX { get; set; }
    public int LocalY { get; set; }
    public int SpriteOffsetX { get; set; }
    public int SpriteOffsetY { get; set; }
}
