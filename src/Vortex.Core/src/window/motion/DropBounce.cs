// @see core/window/motion/DropBounce.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Drop-and-bounce animation. Starts above target position, drops down
/// with bounce easing using a custom piecewise quadratic curve.
/// </summary>
/// @see core/window/motion/DropBounce.as
public class DropBounce : Interval
{
    private readonly int _dropDistance;
    private float _offsetY;

    /// @see DropBounce.as::DropBounce
    public DropBounce(IWindow target, int durationMs, int dropDistance)
        : base(target, durationMs)
    {
        _dropDistance = dropDistance;
    }

    /// @see DropBounce.as::start
    internal override void Start()
    {
        base.Start();

        _offsetY = Target!.y;
        Target!.y = _offsetY - _dropDistance;
    }

    /// @see DropBounce.as::update
    internal override void Update(double progress)
    {
        base.Update(progress);

        Target!.y = _offsetY - _dropDistance + (float)(GetBounceOffset(progress) * _dropDistance);
    }

    /// <summary>
    /// Piecewise quadratic bounce curve with 4 segments.
    /// Coefficient 7.5625 matches standard bounce easing.
    /// </summary>
    /// @see DropBounce.as::getBounceOffset
    protected static double GetBounceOffset(double t)
    {
        switch (t)
        {
            case < 0.364:
                return 7.5625 * t * t;
            case < 0.727:
                t -= 0.545;
                return (7.5625 * t * t) + 0.75;
            case < 0.909:
                t -= 0.9091;
                return (7.5625 * t * t) + 0.9375;
            default:
                t -= 0.955;
                return (7.5625 * t * t) + 0.984375;
        }
    }

    /// @see DropBounce.as::stop
    internal override void Stop()
    {
        if (Target != null)
        {
            Target.y = _offsetY;
        }

        base.Stop();
    }
}
