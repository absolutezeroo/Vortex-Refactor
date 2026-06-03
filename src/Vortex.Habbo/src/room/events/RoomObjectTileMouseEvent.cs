using Vortex.Room.Events;
using Vortex.Room.Object;

namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomObjectTileMouseEvent
public class RoomObjectTileMouseEvent(string type,
    IRoomObject? obj,
    string eventId,
    double tileX,
    double tileY,
    double tileZ,
    bool altKey = false,
    bool ctrlKey = false,
    bool shiftKey = false,
    bool buttonDown = false)
    : RoomObjectMouseEvent(type, obj, eventId, altKey, ctrlKey, shiftKey, buttonDown)
{
    public double TileX { get; } = tileX;

    public double TileY { get; } = tileY;

    public double TileZ { get; } = tileZ;

    public int TileXAsInt => (int)(TileX + 0.499);
    public int TileYAsInt => (int)(TileY + 0.499);
    public int TileZAsInt => (int)(TileZ + 0.499);
}
