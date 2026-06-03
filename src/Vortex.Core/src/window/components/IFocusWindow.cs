// @see core/window/components/class_3403.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/class_3403.as
public interface IFocusWindow
{
    /// @see core/window/components/class_3403.as::get focused
    bool Focused { get; }

    /// @see core/window/components/class_3403.as::focus
    void Focus();

    /// @see core/window/components/class_3403.as::unfocus
    void Unfocus();
}
