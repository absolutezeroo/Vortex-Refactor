// @see core/window/motion/JumpBy.as

using System;

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Jump animation with arc. Sine wave for vertical bounce while moving horizontally.
/// y = startY - peakHeight * abs(sin(progress * PI * bounceCount)) + deltaY * progress
/// </summary>
/// @see core/window/motion/JumpBy.as
public class JumpBy : Interval
{
    protected int _startX;
    protected int _startY;
    protected double _deltaX;
    protected double _deltaY;
    protected double _peakHeight;
    protected int _bounceCount;

    /// @see JumpBy.as::JumpBy
    public JumpBy(IWindow target, int durationMs, int dx, int dy, int height, int bounces)
        : base(target, durationMs)
    {
        _deltaX = dx;
        _deltaY = dy;
        _peakHeight = -height;
        _bounceCount = bounces;
    }

    /// @see JumpBy.as::start
    internal override void Start()
    {
        base.Start();
        _startX = (int)Target!.x;
        _startY = (int)Target!.y;
    }

    /// @see JumpBy.as::update
    internal override void Update(double progress)
    {
        base.Update(progress);
        Target!.x = (float)(_startX + (_deltaX * progress));
        Target!.y = (float)(_startY + (_peakHeight * Math.Abs(Math.Sin(progress * Math.PI * _bounceCount))) + (_deltaY * progress));
    }
}
