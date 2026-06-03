namespace Vortex.Habbo.Room.Events;

using Vortex.Room.Events;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.events.RoomObjectHSLColorEnableEvent
public class RoomObjectHSLColorEnableEvent(string type,
    IRoomObject? obj,
    bool enable,
    int hue,
    int saturation,
    int lightness)
    : RoomObjectEvent(type, obj)
{
    public const string ROOM_BACKGROUND_COLOR = "ROHSLCEE_ROOM_BACKGROUND_COLOR";

    public bool Enable => enable;

    public int Hue => hue;

    public int Saturation => saturation;

    public int Lightness => lightness;
}
