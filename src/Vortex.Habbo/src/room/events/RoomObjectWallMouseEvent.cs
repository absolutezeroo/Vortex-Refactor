using Vortex.Room.Events;
using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomObjectWallMouseEvent
public class RoomObjectWallMouseEvent : RoomObjectMouseEvent
{
    private readonly Vector3d _wallLocation;
    private readonly Vector3d _wallWidth;
    private readonly Vector3d _wallHeight;

    public RoomObjectWallMouseEvent(
        string type,
        IRoomObject? obj,
        string eventId,
        IVector3d wallLocation,
        IVector3d wallWidth,
        IVector3d wallHeight,
        double x,
        double y,
        double direction,
        bool altKey = false,
        bool ctrlKey = false,
        bool shiftKey = false,
        bool buttonDown = false)
        : base(type, obj, eventId, altKey, ctrlKey, shiftKey, buttonDown)
    {
        _wallLocation = new Vector3d();
        _wallLocation.Assign(wallLocation);
        _wallWidth = new Vector3d();
        _wallWidth.Assign(wallWidth);
        _wallHeight = new Vector3d();
        _wallHeight.Assign(wallHeight);
        X = x;
        Y = y;
        Direction = direction;
    }

    public IVector3d WallLocation => _wallLocation;
    public IVector3d WallWidth => _wallWidth;
    public IVector3d WallHeight => _wallHeight;
    public double X { get; }

    public double Y { get; }

    public double Direction { get; }
}
