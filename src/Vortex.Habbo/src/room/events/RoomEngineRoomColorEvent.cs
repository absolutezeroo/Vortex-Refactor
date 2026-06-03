namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineRoomColorEvent
public class RoomEngineRoomColorEvent(int roomId, uint color, uint brightness, bool bgOnly)
    : RoomEngineEvent(ROOM_COLOR, roomId)
{
    public const string ROOM_COLOR = "REE_ROOM_COLOR";

    public uint Color { get; } = color;

    public uint Brightness { get; } = brightness;

    public bool BgOnly { get; } = bgOnly;
}
