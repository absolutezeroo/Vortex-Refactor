using Vortex.Room.Object;

namespace Vortex.Room;

/// @see com.sulake.room.IRoomObjectManager
public interface IRoomObjectManager
{
    void Dispose();
    IRoomObjectController? CreateObject(int id, uint stateCount, string type);
    IRoomObjectController? GetObject(int id);
    List<IRoomObjectController>? GetObjects();
    bool DisposeObject(int id);
    int ObjectCount { get; }
    IRoomObjectController? GetObjectWithIndex(int index);
    int GetObjectCountForType(string type);
    IRoomObjectController? GetObjectWithIndexAndType(int index, string type);
    void Reset();
}
