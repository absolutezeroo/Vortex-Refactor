// @see core/window/components/IItemGridWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IItemGridWindow.as
public interface IItemGridWindow
{
    /// @see core/window/components/IItemGridWindow.as::get/set spacing
    int Spacing { get; set; }

    /// @see core/window/components/IItemGridWindow.as::get/set verticalSpacing
    int VerticalSpacing { get; set; }

    /// @see core/window/components/IItemGridWindow.as::get/set scaleToFitItems
    bool ScaleToFitItems { get; set; }

    /// @see core/window/components/IItemGridWindow.as::get/set autoArrangeItems
    bool AutoArrangeItems { get; set; }

    /// @see core/window/components/IItemGridWindow.as::get/set resizeOnItemUpdate
    bool ResizeOnItemUpdate { get; set; }

    /// @see core/window/components/IItemGridWindow.as::get numColumns
    int NumColumns { get; }

    /// @see core/window/components/IItemGridWindow.as::get numRows
    int NumRows { get; }

    /// @see core/window/components/IItemGridWindow.as::get numGridItems
    int NumGridItems { get; }

    /// @see core/window/components/IItemGridWindow.as::get/set shouldRebuildGridOnResize
    bool ShouldRebuildGridOnResize { get; set; }

    /// @see core/window/components/IItemGridWindow.as::get/set containerResizeToColumns
    bool ContainerResizeToColumns { get; set; }

    /// @see core/window/components/IItemGridWindow.as::addGridItem
    IWindow? AddGridItem(IWindow item);

    /// @see core/window/components/IItemGridWindow.as::addGridItemAt
    IWindow? AddGridItemAt(IWindow item, int index);

    /// @see core/window/components/IItemGridWindow.as::getGridItemAt
    IWindow? GetGridItemAt(int index);

    /// @see core/window/components/IItemGridWindow.as::getGridItemByID
    IWindow? GetGridItemByID(uint id);

    /// @see core/window/components/IItemGridWindow.as::getGridItemByName
    IWindow? GetGridItemByName(string name);

    /// @see core/window/components/IItemGridWindow.as::getGridItemByTag
    IWindow? GetGridItemByTag(string tag);

    /// @see core/window/components/IItemGridWindow.as::getGridItemIndex
    int GetGridItemIndex(IWindow item);

    /// @see core/window/components/IItemGridWindow.as::removeGridItem
    IWindow? RemoveGridItem(IWindow item);

    /// @see core/window/components/IItemGridWindow.as::removeGridItemAt
    IWindow? RemoveGridItemAt(int index);

    /// @see core/window/components/IItemGridWindow.as::setGridItemIndex
    void SetGridItemIndex(IWindow item, int index);

    /// @see core/window/components/IItemGridWindow.as::swapGridItems
    void SwapGridItems(IWindow item1, IWindow item2);

    /// @see core/window/components/IItemGridWindow.as::swapGridItemsAt
    void SwapGridItemsAt(int index1, int index2);

    /// @see core/window/components/IItemGridWindow.as::removeGridItems
    void RemoveGridItems();

    /// @see core/window/components/IItemGridWindow.as::destroyGridItems
    void DestroyGridItems();

    /// @see core/window/components/IItemGridWindow.as::rebuildGridStructure
    void RebuildGridStructure();

    /// @see core/window/components/IItemGridWindow.as::populate
    void Populate(IList<IWindow> items);
}
