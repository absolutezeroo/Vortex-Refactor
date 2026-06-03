// @see core/window/motion/MoveBy.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Animates window by relative offset (dx, dy) over duration.
/// Converts relative to absolute on start, then delegates to MoveTo.
/// </summary>
/// @see core/window/motion/MoveBy.as
public class MoveBy : MoveTo
{
    /// @see MoveBy.as::MoveBy
    public MoveBy(IWindow target, int durationMs, int dx, int dy) : base(target, durationMs, dx, dy) { }

    /// @see MoveBy.as::start
    internal override void Start()
    {
        _targetX = Target!.x + _targetX;
        _targetY = Target!.y + _targetY;

        base.Start();
    }
}
