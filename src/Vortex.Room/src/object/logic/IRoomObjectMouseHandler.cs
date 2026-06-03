using Vortex.Room.Events;
using Vortex.Room.Utils;

namespace Vortex.Room.Object.Logic;

/// @see com.sulake.room.object.logic.IRoomObjectMouseHandler
public interface IRoomObjectMouseHandler
{
    void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry);
}
