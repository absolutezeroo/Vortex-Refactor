// @see core/window/services/WindowMouseScaler.as

namespace Vortex.Core.Window.Services;

/// <summary>
/// Mouse scaling service. Extends WindowMouseOperator to scale windows
/// horizontally and/or vertically based on mouse delta.
/// Flags: 4096 = horizontal scaling, 8192 = vertical scaling.
/// </summary>
/// @see core/window/services/WindowMouseScaler.as
public class WindowMouseScaler : WindowMouseOperator
{
    /// @see WindowMouseScaler.as::WindowMouseScaler
    public WindowMouseScaler() : base() { }

    /// @see WindowMouseScaler.as::operate
    public override void Operate(int mouseX, int mouseY)
    {
        if (_window == null || _window.disposed)
        {
            return;
        }

        int dx = (_flags & 4096) != 0 ? mouseX - (int)_mouse.X : 0;
        int dy = (_flags & 8192) != 0 ? mouseY - (int)_mouse.Y : 0;

        // @see WindowMouseScaler.as::operate — delegate to Scale()
        _window.Scale(dx, dy);
    }
}
