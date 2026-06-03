using Vortex.Room.Events;
using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Room.Renderer;

/// @see com.sulake.room.renderer.IRoomRenderingCanvasMouseListener
public interface IRoomRenderingCanvasMouseListener
{
    void ProcessRoomCanvasMouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomObject? obj, IRoomGeometry geometry);
}
