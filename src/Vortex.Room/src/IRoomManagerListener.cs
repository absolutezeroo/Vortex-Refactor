namespace Vortex.Room;

/// @see com.sulake.room.IRoomManagerListener
public interface IRoomManagerListener
{
    void RoomManagerInitialized(bool success);
    void ContentLoaded(string type, bool success);
    void ObjectInitialized(string roomId, int objectId, int category);
    void ObjectsInitialized(string roomId);
}
