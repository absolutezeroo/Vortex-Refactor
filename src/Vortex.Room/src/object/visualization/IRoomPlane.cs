using Vortex.Room.Utils;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.IRoomPlane
public interface IRoomPlane
{
    int UniqueId { get; }
    IVector3d Location { get; }
    IVector3d LeftSide { get; }
    IVector3d RightSide { get; }
    uint Color { get; }
    List<object>? GetDrawingDatas(IRoomGeometry geometry);
}
