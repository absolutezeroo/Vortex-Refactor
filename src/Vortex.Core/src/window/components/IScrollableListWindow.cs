// @see core/window/components/IScrollableListWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IScrollableListWindow.as
public interface IScrollableListWindow
{
    /// @see core/window/components/IScrollableListWindow.as::get/set autoHideScrollBar
    bool AutoHideScrollBar { get; set; }

    /// @see core/window/components/IScrollableListWindow.as::get isScrollBarVisible
    bool IsScrollBarVisible { get; }
}
