// @see core/window/components/IInteractiveWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IInteractiveWindow.as
public interface IInteractiveWindow : IWindow
{
    /// @see core/window/components/IInteractiveWindow.as::get/set toolTipCaption
    string? ToolTipCaption { get; set; }

    /// @see core/window/components/IInteractiveWindow.as::get/set toolTipDelay
    uint ToolTipDelay { get; set; }

    /// @see core/window/components/IInteractiveWindow.as::get/set toolTipIsDynamic
    bool ToolTipIsDynamic { get; set; }

    /// @see core/window/components/IInteractiveWindow.as::get/set interactiveCursorDisabled
    bool InteractiveCursorDisabled { get; set; }

    /// @see core/window/components/IInteractiveWindow.as::showToolTip
    void ShowToolTip();

    /// @see core/window/components/IInteractiveWindow.as::hideToolTip
    void HideToolTip();

    /// @see core/window/components/IInteractiveWindow.as::setMouseCursorForState
    uint SetMouseCursorForState(uint state, uint cursor);

    /// @see core/window/components/IInteractiveWindow.as::getMouseCursorByState
    uint GetMouseCursorByState(uint state);
}
