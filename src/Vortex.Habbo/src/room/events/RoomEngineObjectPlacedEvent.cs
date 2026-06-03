namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineObjectPlacedEvent
public class RoomEngineObjectPlacedEvent(string type,
    int roomId,
    int objectId,
    int category,
    string wallLocation,
    double x,
    double y,
    double z,
    int direction,
    bool placedInRoom,
    bool placedOnFloor,
    bool placedOnWall,
    string? instanceData)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public string WallLocation { get; } = wallLocation;

    public double X { get; } = x;

    public double Y { get; } = y;

    public double Z { get; } = z;

    public int Direction { get; } = direction;

    public bool PlacedInRoom { get; } = placedInRoom;

    public bool PlacedOnFloor { get; } = placedOnFloor;

    public bool PlacedOnWall { get; } = placedOnWall;

    public string? InstanceData { get; } = instanceData;
}
