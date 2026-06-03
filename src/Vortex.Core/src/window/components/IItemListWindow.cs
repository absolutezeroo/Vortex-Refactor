// @see core/window/components/IItemListWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IItemListWindow.as
public interface IItemListWindow
{
    /// @see core/window/components/IItemListWindow.as::get/set spacing
    int Spacing { get; set; }

    /// @see core/window/components/IItemListWindow.as::get/set scaleToFitItems
    bool ScaleToFitItems { get; set; }

    /// @see core/window/components/IItemListWindow.as::get/set autoArrangeItems
    bool AutoArrangeItems { get; set; }

    /// @see core/window/components/IItemListWindow.as::get/set resizeOnItemUpdate
    bool ResizeOnItemUpdate { get; set; }

    /// @see core/window/components/IItemListWindow.as::get numListItems
    int NumListItems { get; }

    /// @see core/window/components/IItemListWindow.as::get/set isPartOfGridWindow
    bool IsPartOfGridWindow { get; set; }

    /// @see core/window/components/IItemListWindow.as::get scrollableWindow
    IWindow? ScrollableWindow { get; }

    /// @see core/window/components/IItemListWindow.as::set disableAutodrag
    bool DisableAutodrag { set; }

    /// @see core/window/components/IItemListWindow.as::addListItem
    IWindow? AddListItem(IWindow item);

    /// @see core/window/components/IItemListWindow.as::addListItemAt
    IWindow? AddListItemAt(IWindow item, int index);

    /// @see core/window/components/IItemListWindow.as::getListItemAt
    IWindow? GetListItemAt(int index);

    /// @see core/window/components/IItemListWindow.as::getListItemByID
    IWindow? GetListItemByID(uint id);

    /// @see core/window/components/IItemListWindow.as::getListItemByName
    IWindow? GetListItemByName(string name);

    /// @see core/window/components/IItemListWindow.as::getListItemByTag
    IWindow? GetListItemByTag(string tag);

    /// @see core/window/components/IItemListWindow.as::getListItemIndex
    int GetListItemIndex(IWindow item);

    /// @see core/window/components/IItemListWindow.as::removeListItem
    IWindow? RemoveListItem(IWindow item);

    /// @see core/window/components/IItemListWindow.as::removeListItemAt
    IWindow? RemoveListItemAt(int index);

    /// @see core/window/components/IItemListWindow.as::setListItemIndex
    void SetListItemIndex(IWindow item, int index);

    /// @see core/window/components/IItemListWindow.as::swapListItems
    void SwapListItems(IWindow item1, IWindow item2);

    /// @see core/window/components/IItemListWindow.as::groupListItemsWithID
    void GroupListItemsWithId(uint id, List<IWindow> results);

    /// @see core/window/components/IItemListWindow.as::groupListItemsWithTag
    void GroupListItemsWithTag(string tag, List<IWindow> results);

    /// @see core/window/components/IItemListWindow.as::swapListItemsAt
    void SwapListItemsAt(int index1, int index2);

    /// @see core/window/components/IItemListWindow.as::removeListItems
    void RemoveListItems();

    /// @see core/window/components/IItemListWindow.as::destroyListItems
    void DestroyListItems();

    /// @see core/window/components/IItemListWindow.as::arrangeListItems
    void ArrangeListItems();

    /// @see core/window/components/IItemListWindow.as::populate
    void Populate(IList<IWindow> items);

    /// @see core/window/components/IItemListWindow.as::stopDragging
    void StopDragging();

    /// @see core/window/components/IItemListWindow.as::scrollWithWheel
    void ScrollWithWheel(float delta);
}
