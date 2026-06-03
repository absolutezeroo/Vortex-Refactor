// @see core/window/motion/EaseOut.as

using System;

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Ease-out animation. Applies power easing: Math.Pow(progress, 1/rate).
/// </summary>
/// @see core/window/motion/EaseOut.as
public class EaseOut : EaseRate
{
    /// @see EaseOut.as::EaseOut
    public EaseOut(Interval inner, double rate) : base(inner, rate) { }

    /// @see EaseOut.as::update
    internal override void Update(double progress)
    {
        _inner.Update(Math.Pow(progress, 1.0 / _rate));
    }
}
