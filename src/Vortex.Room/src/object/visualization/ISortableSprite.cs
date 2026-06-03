namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.ISortableSprite
public interface ISortableSprite
{
    int X { get; }
    int Y { get; }
    double Z { get; }
    IRoomObjectSprite? Sprite { get; }
}
