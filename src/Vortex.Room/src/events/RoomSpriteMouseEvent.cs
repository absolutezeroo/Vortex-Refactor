namespace Vortex.Room.Events;

/// <summary>
/// Low-level sprite mouse event with screen/local coordinates and sprite tag.
/// Not a Flash Event — plain C# data class.
/// </summary>
/// @see com.sulake.room.events.RoomSpriteMouseEvent
public class RoomSpriteMouseEvent(string type,
    string eventId,
    string canvasId,
    string spriteTag,
    double screenX,
    double screenY,
    double localX = 0,
    double localY = 0,
    bool ctrlKey = false,
    bool altKey = false,
    bool shiftKey = false,
    bool buttonDown = false)
{
    public string Type { get; } = type;
    public string EventId { get; } = eventId;
    public string CanvasId { get; } = canvasId;
    public string SpriteTag { get; } = spriteTag;
    public double ScreenX { get; } = screenX;
    public double ScreenY { get; } = screenY;
    public double LocalX { get; } = localX;
    public double LocalY { get; } = localY;
    public bool CtrlKey { get; } = ctrlKey;
    public bool AltKey { get; } = altKey;
    public bool ShiftKey { get; } = shiftKey;
    public bool ButtonDown { get; } = buttonDown;
    public int SpriteOffsetX { get; set; }
    public int SpriteOffsetY { get; set; }
}
