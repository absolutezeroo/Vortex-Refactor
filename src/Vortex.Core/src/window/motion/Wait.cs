// @see core/window/motion/Wait.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Wait/delay motion. Null target. Marks complete when elapsed time exceeds duration.
/// </summary>
/// @see core/window/motion/Wait.as
public class Wait : Motion
{
    private long _startTimeMs;
    private readonly int _waitDurationMs;

    /// @see Wait.as::Wait
    public Wait(int durationMs) : base(null)
    {
        _waitDurationMs = durationMs;
    }

    /// @see Wait.as::get running
    public override bool Running => _running;

    /// @see Wait.as::start
    internal override void Start()
    {
        base.Start();

        _complete = false;
        _startTimeMs = MotionManager.GetTimer();
    }

    /// @see Wait.as::tick
    internal override void Tick(long currentTimeMs)
    {
        _complete = currentTimeMs - _startTimeMs >= _waitDurationMs;

        if (_complete)
        {
            Stop();
        }

        base.Tick(currentTimeMs);
    }
}
