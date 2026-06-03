// @see core/window/motion/Interval.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Duration-based motion. Computes normalized progress [0,1] from elapsed time
/// and calls Update() each tick. Marks complete when progress >= 1.
/// </summary>
/// @see core/window/motion/Interval.as
public class Interval : Motion
{
    private long _startTimeMs;

    /// @see Interval.as::Interval
    public Interval(IWindow? target, int durationMs) : base(target)
    {
        _complete = false;
        Duration = durationMs;
    }

    /// @see Interval.as::get duration
    public int Duration { get; }

    /// @see Interval.as::start
    internal override void Start()
    {
        base.Start();

        _complete = false;
        _startTimeMs = MotionManager.GetTimer();
    }

    /// @see Interval.as::tick
    internal sealed override void Tick(long currentTimeMs)
    {
        double progress = (double)(currentTimeMs - _startTimeMs) / Duration;

        if (progress < 1.0)
        {
            Update(progress);
        }
        else
        {
            Update(1.0);
            _complete = true;
        }
    }
}
