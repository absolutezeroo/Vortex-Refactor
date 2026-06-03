namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureBottleVisualization
public class FurnitureBottleVisualization : AnimatedFurnitureVisualization
{
    private const int ANIMATION_ID_OFFSET_SLOW1 = 20;
    private const int ANIMATION_ID_OFFSET_SLOW2 = 9;

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
                    -1
                };
                return;
            }
        }

        if (animationId >= 0 && animationId <= 7)
        {
            if (_running)
            {
                _running = false;
                _animationQueue = new List<int>
                {
                    ANIMATION_ID_OFFSET_SLOW1, ANIMATION_ID_OFFSET_SLOW2 + animationId, animationId,
                };
                return;
            }

            base.SetAnimation(animationId);
        }
    }

    protected override int UpdateAnimation(double scale)
    {
        if (GetLastFramePlayed(0))
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
