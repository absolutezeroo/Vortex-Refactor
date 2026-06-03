namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.IRoomObjectSpriteVisualization
public interface IRoomObjectSpriteVisualization : IRoomObjectGraphicVisualization
{
    int SpriteCount { get; }
    IRoomObjectSprite? GetSprite(int index);
    List<IRoomObjectSprite>? GetSpriteList();
}
