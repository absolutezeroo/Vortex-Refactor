// @see core/window/components/ISelectorWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ISelectorWindow.as
public interface ISelectorWindow
{
    /// @see core/window/components/ISelectorWindow.as::get numSelectables
    int NumSelectables { get; }

    /// @see core/window/components/ISelectorWindow.as::getSelected
    ISelectableWindow? GetSelected();

    /// @see core/window/components/ISelectorWindow.as::setSelected
    void SetSelected(ISelectableWindow? window);

    /// @see core/window/components/ISelectorWindow.as::addSelectable
    ISelectableWindow? AddSelectable(ISelectableWindow window);

    /// @see core/window/components/ISelectorWindow.as::addSelectableAt
    ISelectableWindow? AddSelectableAt(ISelectableWindow window, int index);

    /// @see core/window/components/ISelectorWindow.as::getSelectableAt
    ISelectableWindow? GetSelectableAt(int index);

    /// @see core/window/components/ISelectorWindow.as::getSelectableByID
    ISelectableWindow? GetSelectableByID(uint id);

    /// @see core/window/components/ISelectorWindow.as::getSelectableByTag
    ISelectableWindow? GetSelectableByTag(string tag);

    /// @see core/window/components/ISelectorWindow.as::getSelectableByName
    ISelectableWindow? GetSelectableByName(string name);

    /// @see core/window/components/ISelectorWindow.as::getSelectableIndex
    int GetSelectableIndex(ISelectableWindow window);

    /// @see core/window/components/ISelectorWindow.as::removeSelectable
    ISelectableWindow? RemoveSelectable(ISelectableWindow window);
}
