using Godot;

using Vortex.Room.Utils;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.IRoomObjectVisualization
public interface IRoomObjectVisualization
{
    IRoomObject? Object { get; set; }
    void Dispose();
    bool Initialize(IRoomObjectVisualizationData data);
    void Update(IRoomGeometry geometry, int time, bool full, bool skip);
    Image? GetImage();
    Image? GetImage(int width, int height);
    Rect2I BoundingRectangle { get; }
    int InstanceId { get; }
    int UpdateId { get; }
}
