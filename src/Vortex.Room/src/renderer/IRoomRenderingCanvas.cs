using Godot;

using Vortex.Room.Data;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Utils;

namespace Vortex.Room.Renderer;

/// @see com.sulake.room.renderer.IRoomRenderingCanvas
public interface IRoomRenderingCanvas
{
    bool UseMask { set; }
    void Initialize(int width, int height);
    int Width { get; }
    int Height { get; }
    int ScreenOffsetX { get; set; }
    int ScreenOffsetY { get; set; }
    void Render(int time, bool update = false);
    Node2D? DisplayObject { get; }
    IRoomGeometry? Geometry { get; }
    IRoomRenderingCanvasMouseListener? MouseListener { set; }
    bool HandleMouseEvent(int x, int y, string type, bool altKey, bool ctrlKey, bool shiftKey, bool buttonDown);
    List<RoomObjectSpriteData>? GetSortableSpriteList();
    List<ISortableSprite>? GetPlaneSortableSprites();
    void SetScale(double scale, Vector2? point = null, Vector2? offset = null, bool skipUpdate = false);
    double Scale { get; }
    Image? TakeScreenShot();
    void SkipSpriteVisibilityChecking();
    void ResumeSpriteVisibilityChecking();
    int Id { get; }
}
