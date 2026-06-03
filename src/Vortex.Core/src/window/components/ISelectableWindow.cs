// @see core/window/components/ISelectableWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ISelectableWindow.as
public interface ISelectableWindow : IWindow
{
    /// @see core/window/components/ISelectableWindow.as::get selector
    IWindow? Selector { get; }

    /// @see core/window/components/ISelectableWindow.as::get/set isSelected
    bool IsSelected { get; set; }

    /// @see core/window/components/ISelectableWindow.as::select
    bool Select();

    /// @see core/window/components/ISelectableWindow.as::unselect
    bool Unselect();
}
