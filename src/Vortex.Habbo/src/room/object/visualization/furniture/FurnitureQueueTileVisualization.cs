namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureQueueTileVisualization
public class FurnitureQueueTileVisualization : AnimatedFurnitureVisualization
{
    private const int ANIMATION_ID_TRIGGERED = 2;
    private const int ANIMATION_ID_NORMAL = 1;
    private const int COUNTDOWN_FRAMES = 15;

    private List<int> _animationQueue = new();
    private int _countdown;

    protected override void SetAnimation(int animationId)
    {
        if (animationId == ANIMATION_ID_TRIGGERED)
        {
            _animationQueue = new List<int> { ANIMATION_ID_NORMAL };
            _countdown = COUNTDOWN_FRAMES;
        }

        base.SetAnimation(animationId);
    }

    protected override int UpdateAnimation(double scale)
    {
        if (_countdown > 0)
        {
            _countdown--;
        }

        if (_countdown == 0)
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

    protected override bool UsesAnimationResetting()
    {
        return true;
    }
}
