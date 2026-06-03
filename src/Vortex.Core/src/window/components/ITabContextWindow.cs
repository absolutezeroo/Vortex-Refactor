// @see core/window/components/ITabContextWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ITabContextWindow.as
public interface ITabContextWindow
{
    /// @see core/window/components/ITabContextWindow.as::get selector
    ISelectorWindow? Selector { get; }

    /// @see core/window/components/ITabContextWindow.as::get container
    IWindow? Container { get; }

    /// @see core/window/components/ITabContextWindow.as::get numTabItems
    int NumTabItems { get; }

    /// @see core/window/components/ITabContextWindow.as::addTabItem
    ISelectableWindow? AddTabItem(ISelectableWindow item);

    /// @see core/window/components/ITabContextWindow.as::addTabItemAt
    ISelectableWindow? AddTabItemAt(ISelectableWindow item, int index);

    /// @see core/window/components/ITabContextWindow.as::removeTabItem
    ISelectableWindow? RemoveTabItem(ISelectableWindow item);

    /// @see core/window/components/ITabContextWindow.as::getTabItemAt
    ISelectableWindow? GetTabItemAt(int index);

    /// @see core/window/components/ITabContextWindow.as::getTabItemByName
    ISelectableWindow? GetTabItemByName(string name);

    /// @see core/window/components/ITabContextWindow.as::getTabItemByID
    ISelectableWindow? GetTabItemByID(uint id);

    /// @see core/window/components/ITabContextWindow.as::getTabItemIndex
    int GetTabItemIndex(ISelectableWindow item);
}
