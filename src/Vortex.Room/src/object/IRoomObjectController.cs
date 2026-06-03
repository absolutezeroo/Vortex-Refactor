using Vortex.Room.Object.Logic;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Room.Object;

/// @see com.sulake.room.object.IRoomObjectController
public interface IRoomObjectController : IRoomObject
{
    void Dispose();
    void SetInitialized(bool initialized);
    void SetLocation(IVector3d location);
    void SetDirection(IVector3d direction);
    void SetVisualization(IRoomObjectVisualization visualization);
    bool SetState(int index, int value);
    void SetEventHandler(IRoomObjectEventHandler handler);
    IRoomObjectEventHandler? EventHandler { get; }
    IRoomObjectModelController ModelController { get; }
}
