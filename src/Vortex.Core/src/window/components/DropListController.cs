// @see core/window/components/DropListController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/DropListController.as
public class DropListController : DropBaseController, IDropListWindow
{
    /// @see DropListController.as::DropListController (default)
    public DropListController() : base() { }

    /// @see DropListController.as::DropListController (name + rect)
    public DropListController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see DropListController.as::DropListController (full AS3 11-param signature)
    public DropListController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }

    /// @see DropListController.as::get iterator
    public virtual object? Iterator()
    {
        return new DropListIterator(this);
    }

    /// @see DropListController.as::addMenuItem
    public IWindow? AddMenuItem(IWindow item)
    {
        // @see DropListController.as — skip special named children
        if (item.name is TEXT_FIELD_NAME or ITEM_LIST_NAME or REGION_NAME)
        {
            return item;
        }

        _itemArray.Add(item);

        // If menu is open, reopen to refresh
        if (!_expanded)
        {
            return item;
        }

        CloseExpandedMenuView();
        OpenExpandedMenuView();

        return item;
    }

    /// @see DropListController.as::addMenuItemAt
    public IWindow? AddMenuItemAt(IWindow item, int index)
    {
        if (item.name is TEXT_FIELD_NAME or ITEM_LIST_NAME or REGION_NAME)
        {
            return item;
        }

        if (index < 0 || index > _itemArray.Count)
        {
            index = _itemArray.Count;
        }

        _itemArray.Insert(index, item);

        if (!_expanded)
        {
            return item;
        }

        CloseExpandedMenuView();
        OpenExpandedMenuView();

        return item;
    }

    /// @see DropListController.as::getMenuItemAt
    public IWindow? GetMenuItemAt(int index)
    {
        if (index < 0 || index >= _itemArray.Count)
        {
            return null;
        }

        return _itemArray[index];
    }

    /// @see DropListController.as::removeMenuItem
    public IWindow? RemoveMenuItem(IWindow item)
    {
        int index = _itemArray.IndexOf(item);

        if (index < 0)
        {
            return null;
        }

        _itemArray.RemoveAt(index);

        // @see DropListController.as — adjust selection index
        if (_selectionIndex >= _itemArray.Count)
        {
            _selectionIndex = _itemArray.Count - 1;
        }

        return item;
    }

    /// @see DropListController.as::removeMenuItemAt
    public IWindow? RemoveMenuItemAt(int index)
    {
        if (index < 0 || index >= _itemArray.Count)
        {
            return null;
        }

        IWindow item = _itemArray[index];

        _itemArray.RemoveAt(index);

        if (_selectionIndex >= _itemArray.Count)
        {
            _selectionIndex = _itemArray.Count - 1;
        }

        return item;
    }

    /// @see DropListController.as::getMenuItemIndex
    public int GetMenuItemIndex(IWindow item)
    {
        return _itemArray.IndexOf(item);
    }
}
