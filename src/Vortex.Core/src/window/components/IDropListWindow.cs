// @see core/window/components/class_3539.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/class_3539.as
public interface IDropListWindow
{
    /// @see core/window/components/class_3539.as::get/set selection
    int Selection { get; set; }

    /// @see core/window/components/class_3539.as::get numMenuItems
    int NumMenuItems { get; }

    /// @see core/window/components/class_3539.as::addMenuItem
    IWindow? AddMenuItem(IWindow item);

    /// @see core/window/components/class_3539.as::addMenuItemAt
    IWindow? AddMenuItemAt(IWindow item, int index);

    /// @see core/window/components/class_3539.as::getMenuItemIndex
    int GetMenuItemIndex(IWindow item);

    /// @see core/window/components/class_3539.as::getMenuItemAt
    IWindow? GetMenuItemAt(int index);

    /// @see core/window/components/class_3539.as::removeMenuItem
    IWindow? RemoveMenuItem(IWindow item);

    /// @see core/window/components/class_3539.as::removeMenuItemAt
    IWindow? RemoveMenuItemAt(int index);
}
