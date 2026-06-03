// @see core/window/motion/EaseRate.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Base class for rate-based easing. Stores an easing rate parameter.
/// </summary>
/// @see core/window/motion/EaseRate.as
public class EaseRate : Ease
{
    protected double _rate;

    /// @see EaseRate.as::EaseRate
    public EaseRate(Interval inner, double rate) : base(inner)
    {
        _rate = rate;
    }
}
