using Vortex.Room.Object;

namespace Vortex.Room.Renderer;

/// @see com.sulake.room.renderer.IRoomRendererBase
public interface IRoomRendererBase
{
    void Dispose();
    void Reset();
    void FeedRoomObject(IRoomObject obj);
    void RemoveRoomObject(IRoomObject obj);
    void Update(uint time);
}
