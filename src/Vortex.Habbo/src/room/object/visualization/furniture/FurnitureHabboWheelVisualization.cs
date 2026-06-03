namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureHabboWheelVisualization
public class FurnitureHabboWheelVisualization : AnimatedFurnitureVisualization
{
    private const int ANIMATION_ID_OFFSET_SLOW1 = 10;
    private const int ANIMATION_ID_OFFSET_SLOW2 = 20;
    private const int ANIMATION_ID_ROLL = 31;
    private const int ANIMATION_ID_STOP = 32;

    private List<int> _animationQueue = new();
    private bool _running;

    protected override void SetAnimation(int animationId)
    {
        if (animationId == -1)
        {
            if (!_running)
            {
                _running = true;
                _animationQueue = new List<int>
                {
                    ANIMATION_ID_ROLL, ANIMATION_ID_STOP
                };
                return;
            }
        }

        if (animationId > 0 && animationId <= 10)
        {
            if (_running)
            {
                _running = false;
                _animationQueue = new List<int>
                {
                    ANIMATION_ID_OFFSET_SLOW1 + animationId, ANIMATION_ID_OFFSET_SLOW2 + animationId, animationId,
                };
                return;
            }

            base.SetAnimation(animationId);
        }
    }

    protected override int UpdateAnimation(double scale)
    {
        if (GetLastFramePlayed(1) && GetLastFramePlayed(2) && GetLastFramePlayed(3))
        {
            if (_animationQueue.Count > 0)
            {
                int next = _animationQueue[0];
                _animationQueue.RemoveAt(0);
                base.SetAnimation(next);
            }
        }

        return base.UpdateAnimation(scale);
    }
}
