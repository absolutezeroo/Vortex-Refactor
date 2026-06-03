using Vortex.Room.Object;

namespace Vortex.Room;

/// @see com.sulake.room.IRoomInstanceContainer
public interface IRoomInstanceContainer
{
    IRoomObject? CreateRoomObject(string roomId, int objectId, string type, int category);
    IRoomObjectManager CreateRoomObjectManager();
}
