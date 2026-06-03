using System.Xml.Linq;

namespace Vortex.Room;

/// @see com.sulake.room.IRoomManager
public interface IRoomManager
{
    bool Initialize(XElement? xml, IRoomManagerListener listener);
    void Update(uint time);
    void SetContentLoader(IRoomContentLoader loader);
    void AddObjectUpdateCategory(int category);
    void RemoveObjectUpdateCategory(int category);
    IRoomInstance? CreateRoom(string id, XElement? xml);
    bool DisposeRoom(string id);
    IRoomInstance? GetRoom(string id);
    IRoomInstance? GetRoomWithIndex(int index);
    int RoomCount { get; }
    bool IsContentAvailable(string type);
}
