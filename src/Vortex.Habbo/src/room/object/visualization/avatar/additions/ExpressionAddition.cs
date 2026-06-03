using Vortex.Room.Object.Visualization;

namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.ExpressionAddition
public class ExpressionAddition(int id, int type, AvatarVisualization visualization) : IExpressionAddition
{
    protected int _id = id;
    protected AvatarVisualization? _visualization = visualization;

    public int Type { get; } = type;

    public int Id => _id;

    public bool disposed => _visualization == null;

    public void Dispose()
    {
        _visualization = null;
    }

    public virtual void Update(IRoomObjectSprite sprite, double scale)
    {
    }

    public virtual bool Animate(IRoomObjectSprite sprite)
    {
        return false;
    }
}
