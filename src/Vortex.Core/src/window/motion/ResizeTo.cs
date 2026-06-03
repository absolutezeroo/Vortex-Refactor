// @see core/window/motion/ResizeTo.as

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Animates window resize to target width/height over duration.
/// Linear interpolation on width and height.
/// </summary>
/// @see core/window/motion/ResizeTo.as
public class ResizeTo : Interval
{
    protected double _startWidth;
    protected double _startHeight;
    protected double _targetWidth;
    protected double _targetHeight;
    protected double _deltaWidth;
    protected double _deltaHeight;

    /// @see ResizeTo.as::ResizeTo
    public ResizeTo(IWindow target, int durationMs, int targetWidth, int targetHeight)
        : base(target, durationMs)
    {
        _targetWidth = targetWidth;
        _targetHeight = targetHeight;
    }

    /// @see ResizeTo.as::start
    internal override void Start()
    {
        base.Start();

        _startWidth = Target!.width;
        _startHeight = Target!.height;
        _deltaWidth = _targetWidth - _startWidth;
        _deltaHeight = _targetHeight - _startHeight;
    }

    /// @see ResizeTo.as::update
    internal override void Update(double progress)
    {
        Target!.width = (float)(_startWidth + (_deltaWidth * progress));
        Target!.height = (float)(_startHeight + (_deltaHeight * progress));
    }
}
