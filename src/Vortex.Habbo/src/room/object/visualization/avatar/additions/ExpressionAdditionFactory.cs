namespace Vortex.Habbo.Room.Object.Visualization.Avatar.Additions;

/// @see com.sulake.habbo.room.object.visualization.avatar.additions.class_3649
public static class ExpressionAdditionFactory
{
    public const int WAVE = 1;
    public const int BLOW = 2;
    public const int LAUGH = 3;
    public const int CRY = 4;
    public const int IDLE = 5;

    public static IExpressionAddition Make(int id, int type, AvatarVisualization visualization)
    {
        if (type == BLOW)
        {
            return new FloatingHeart(id, BLOW, visualization);
        }

        return new ExpressionAddition(id, type, visualization);
    }
}
