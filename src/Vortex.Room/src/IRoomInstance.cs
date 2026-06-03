using Vortex.Room.Object;
using Vortex.Room.Renderer;

namespace Vortex.Room;

/// @see com.sulake.room.IRoomInstance
public interface IRoomInstance
{
    string Id { get; }
    double GetNumber(string key);
    void SetNumber(string key, double value, bool locked = false);
    string? GetString(string key);
    void SetString(string key, string value, bool locked = false);
    void Dispose();
    void Update();
    IRoomObject? CreateRoomObject(int id, string type, int stateCount);
    IRoomObject? GetObject(int id, int category);
    List<IRoomObject>? GetObjects(int category);
    bool DisposeObject(int id, int category);
    int GetObjectCount(int category);
    IRoomObject? GetObjectWithIndexAndType(int index, string type, int category);
    int GetObjectCountForType(string type, int category);
    IRoomObject? GetObjectWithIndex(int index, int category);
    int DisposeObjects(int category);
    void SetRenderer(IRoomRendererBase renderer);
    IRoomRendererBase? GetRenderer();
}
