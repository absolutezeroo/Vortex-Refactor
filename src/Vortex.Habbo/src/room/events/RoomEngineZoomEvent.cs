namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineZoomEvent
public class RoomEngineZoomEvent(int roomId, double level, bool isFlipForced = false) : RoomEngineEvent(ROOM_ZOOM, roomId)
{
    public const string ROOM_ZOOM = "REE_ROOM_ZOOM";

    public double Level { get; } = level;

    public bool IsFlipForced { get; } = isFlipForced;
}
