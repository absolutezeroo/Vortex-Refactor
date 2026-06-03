namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureValRandomizerVisualization
public class FurnitureValRandomizerVisualization : AnimatedFurnitureVisualization
{
    private const int ANIMATION_ID_OFFSET_SLOW1 = 20;
    private const int ANIMATION_ID_OFFSET_SLOW2 = 10;
    private const int ANIMATION_ID_ROLL = 31;
    private const int ANIMATION_ID_STOP = 32;
    private const int ANIMATION_ID_IDLE = 30;

    private List<int> _animationQueue = new();
    private bool _running;

    public FurnitureValRandomizerVisualization()
    {
        base.SetAnimation(ANIMATION_ID_IDLE);
    }

    protected override void SetAnimation(int animationId)
    {
        if (animationId == 0)
        {
            if (!_running)
            {
                _running = true;
                _animationQueue = new List<int> { ANIMATION_ID_ROLL, ANIMATION_ID_STOP };
                return;
            }
        }

        if (animationId > 0 && animationId <= 10)
        {
            if (_running)
            {
                _running = false;
                _animationQueue = new List<int>();

                if (Direction == 2)
                {
                    _animationQueue.Add(ANIMATION_ID_OFFSET_SLOW1 + 5 - animationId);
                    _animationQueue.Add(ANIMATION_ID_OFFSET_SLOW2 + 5 - animationId);
                }
                else
                {
                    _animationQueue.Add(ANIMATION_ID_OFFSET_SLOW1 + animationId);
                    _animationQueue.Add(ANIMATION_ID_OFFSET_SLOW2 + animationId);
                }

                _animationQueue.Add(ANIMATION_ID_IDLE);
                return;
            }

            base.SetAnimation(ANIMATION_ID_IDLE);
        }
    }

    protected override int UpdateAnimation(double scale)
    {
        if (GetLastFramePlayed(11))
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
