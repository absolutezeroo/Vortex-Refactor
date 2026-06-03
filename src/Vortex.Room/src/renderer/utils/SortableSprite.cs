using Vortex.Room.Object.Visualization;

namespace Vortex.Room.Renderer.Utils;

/// @see com.sulake.room.renderer.utils.SortableSprite (class_3707)
public class SortableSprite : ISortableSprite
{
    public const double MAX_Z = 100000000;

    public string Name { get; set; } = "";

    public int X { get; set; }

    public int Y { get; set; }

    public double Z { get; set; }

    public IRoomObjectSprite? Sprite { get; set; }

    public void Dispose()
    {
        Sprite = null;
        Z = -MAX_Z;
    }
}
