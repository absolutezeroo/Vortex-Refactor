using Vortex.Habbo.Room.Object.Visualization.Furniture;

namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.TileCursorVisualization
public class TileCursorVisualization : AnimatedFurnitureVisualization
{
    public double TileHeight { get; set; }

    protected virtual int GetSpriteYOffset(int scale, int direction, int spriteIndex)
    {
        if (spriteIndex != 1)
        {
            return 0;
        }

        TileHeight = Object?.Model.GetNumber("tile_cursor_height") ?? 0;

        return (int)(-TileHeight * (scale / 2.0));

    }
}
