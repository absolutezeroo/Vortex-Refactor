namespace Vortex.Room.Renderer;

/// @see com.sulake.room.renderer.IRoomRenderer
public interface IRoomRenderer : IRoomRendererBase
{
    string? RoomObjectVariableAccurateZ { set; }
    IRoomRenderingCanvas? CreateCanvas(int id, int width, int height, int scale);
    IRoomRenderingCanvas? GetCanvas(int id);
    bool DisposeCanvas(int id);
}
