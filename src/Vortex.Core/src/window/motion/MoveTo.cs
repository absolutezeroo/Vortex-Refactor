// @see core/window/motion/MoveTo.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Animates window to absolute position (targetX, targetY) over duration.
/// Linear interpolation from start position.
/// </summary>
/// @see core/window/motion/MoveTo.as
public class MoveTo : Interval
{
    protected double _startX;
    protected double _startY;
    protected double _targetX;
    protected double _targetY;
    protected double _deltaX;
    protected double _deltaY;

    /// @see MoveTo.as::MoveTo
    public MoveTo(IWindow target, int durationMs, int targetX, int targetY) : base(target, durationMs)
    {
        _targetX = targetX;
        _targetY = targetY;
    }

    /// @see MoveTo.as::start
    internal override void Start()
    {
        base.Start();

        _startX = Target!.x;
        _startY = Target!.y;
        _deltaX = _targetX - _startX;
        _deltaY = _targetY - _startY;
    }

    /// @see MoveTo.as::update
    internal override void Update(double progress)
    {
        Target!.x = (float)(_startX + (_deltaX * progress));
        Target!.y = (float)(_startY + (_deltaY * progress));
    }
}
