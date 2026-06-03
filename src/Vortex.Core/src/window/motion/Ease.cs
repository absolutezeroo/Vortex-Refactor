// @see core/window/motion/Ease.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Decorator for easing. Wraps another Interval and delegates
/// start/update/stop to both itself and the wrapped interval.
/// </summary>
/// @see core/window/motion/Ease.as
public class Ease : Interval
{
    protected Interval _inner;

    /// @see Ease.as::Ease
    public Ease(Interval inner) : base(inner.Target, inner.Duration)
    {
        _inner = inner;
    }

    /// @see Ease.as::start
    internal override void Start()
    {
        base.Start();

        _inner.Start();
    }

    /// @see Ease.as::update
    internal override void Update(double progress)
    {
        base.Update(progress);

        _inner.Update(progress);
    }

    /// @see Ease.as::stop
    internal override void Stop()
    {
        base.Stop();

        _inner.Stop();
    }
}
