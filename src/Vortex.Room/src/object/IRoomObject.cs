using Vortex.Room.Object.Logic;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Room.Object;

/// @see com.sulake.room.object.IRoomObject
public interface IRoomObject
{
    int Id { get; }
    int InstanceId { get; }
    string Type { get; }
    bool IsInitialized { get; }
    IVector3d Location { get; }
    IVector3d Direction { get; }
    IRoomObjectModel Model { get; }
    IRoomObjectVisualization? Visualization { get; }
    IRoomObjectMouseHandler? MouseHandler { get; }
    string? AvatarLibraryAssetName { get; }
    int GetState(int index);
    int UpdateId { get; }
    void TearDown();
}
