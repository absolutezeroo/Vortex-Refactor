namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineHSLColorEnableEvent
public class RoomEngineHSLColorEnableEvent(string type, int roomId, bool enable, int hue, int saturation, int lightness)
    : RoomEngineEvent(type, roomId)
{
    public const string ROOM_BACKGROUND_COLOR = "ROHSLCEE_ROOM_BACKGROUND_COLOR";

    public bool Enable { get; } = enable;

    public int Hue { get; } = hue;

    public int Saturation { get; } = saturation;

    public int Lightness { get; } = lightness;
}
