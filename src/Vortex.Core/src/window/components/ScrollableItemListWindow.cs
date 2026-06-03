// @see core/window/components/ScrollableItemListWindow.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ScrollableItemListWindow.as
/// Thin wrapper around ItemListController + ScrollBarController, found via layout XML tags.
public class ScrollableItemListWindow : WindowController, IScrollableListWindow
{
    private ItemListController? _cachedItemList;
    private ScrollBarController? _cachedScrollBar;
    private bool _autoHideScrollBar = true;

    /// @see ScrollableItemListWindow.as::ScrollableItemListWindow (default)
    public ScrollableItemListWindow() : base() { }

    /// @see ScrollableItemListWindow.as::ScrollableItemListWindow (name + rect)
    public ScrollableItemListWindow(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ScrollableItemListWindow.as::ScrollableItemListWindow (full AS3 11-param signature)
    public ScrollableItemListWindow
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
        // @see ScrollableItemListWindow.as — wire scrollbar to item list
        ScrollBarController? scrollBar = GetScrollBar();
        ItemListController? itemList = GetItemList();

        if (scrollBar != null && itemList is IScrollableWindow scrollable)
        {
            scrollBar.ScrollableTarget = scrollable;
        }
    }

    /// @see ScrollableItemListWindow.as::get _itemList
    protected ItemListController? GetItemList()
    {
        if (_cachedItemList != null)
        {
            return _cachedItemList;
        }

        _cachedItemList = FindChildByTag("_ITEMLIST") as ItemListController;

        return _cachedItemList;
    }

    /// @see ScrollableItemListWindow.as::get _scrollBar
    protected ScrollBarController? GetScrollBar()
    {
        if (_cachedScrollBar != null)
        {
            return _cachedScrollBar;
        }

        _cachedScrollBar = FindChildByTag("_SCROLLBAR") as ScrollBarController;

        return _cachedScrollBar;
    }

    /// @see ScrollableItemListWindow.as::get autoHideScrollBar
    public bool AutoHideScrollBarValue
    {
        get => _autoHideScrollBar;
        set
        {
            _autoHideScrollBar = value;
            UpdateScrollBarVisibility();
        }
    }

    /// @see ScrollableItemListWindow.as::get isScrollBarVisible
    public bool IsScrollBarVisible
    {
        get
        {
            ScrollBarController? sb = GetScrollBar();
            return sb?.visible ?? false;
        }
    }

    /// @see ScrollableItemListWindow.as::get numListItems
    public int NumListItems => GetItemList()?.NumListItems ?? 0;

    /// @see ScrollableItemListWindow.as::get iterator
    public object? ListIterator => GetItemList()?.Iterator();

    /// @see ScrollableItemListWindow.as::get scrollableWindow
    public IWindow? ScrollableWindow => GetItemList();

    public IWindow? AddListItem(IWindow item)
    {
        return GetItemList()?.AddListItem(item);
    }

    public IWindow? AddListItemAt(IWindow item, int index)
    {
        return GetItemList()?.AddListItemAt(item, index);
    }

    public IWindow? GetListItemAt(int index)
    {
        return GetItemList()?.GetListItemAt(index);
    }

    public IWindow? RemoveListItem(IWindow item)
    {
        return GetItemList()?.RemoveListItem(item);
    }

    public void RemoveListItems()
    {
        GetItemList()?.RemoveListItems();
    }

    public void DestroyListItems()
    {
        GetItemList()?.DestroyListItems();
    }

    /// @see ScrollableItemListWindow.as::hideScrollBar
    public void HideScrollBar()
    {
        ScrollBarController? sb = GetScrollBar();

        if (sb != null)
        {
            sb.visible = false;
        }
    }

    /// @see ScrollableItemListWindow.as::showScrollBar
    public void ShowScrollBar()
    {
        ScrollBarController? sb = GetScrollBar();

        if (sb != null)
        {
            sb.visible = true;
        }
    }

    /// @see ScrollableItemListWindow.as::updateScrollBarVisibility
    public void UpdateScrollBarVisibility()
    {
        ScrollBarController? sb = GetScrollBar();

        if (sb == null)
        {
            return;
        }

        if (_autoHideScrollBar && sb.TestStateFlag(STATE_DISABLED))
        {
            HideScrollBar();
        }
        else
        {
            ShowScrollBar();
        }
    }

    /// @see ScrollableItemListWindow.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        ItemListController? il = GetItemList();

        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "spacing":
                    if (prop.value is int s && il != null)
                    {
                        il.Spacing = s;
                    }
                    break;
                case "scale_to_fit_items":
                    if (prop.value is bool sf && il != null)
                    {
                        il.ScaleToFitItems = sf;
                    }
                    break;
                case "resize_on_item_update":
                    if (prop.value is bool ri && il != null)
                    {
                        il.ResizeOnItemUpdate = ri;
                    }
                    break;
                case "auto_arrange_items":
                    if (prop.value is bool aa && il != null)
                    {
                        il.AutoArrangeItems = aa;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see ScrollableItemListWindow.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        _cachedItemList = null;
        _cachedScrollBar = null;

        return base.Destroy();
    }

    bool IScrollableListWindow.AutoHideScrollBar { get => _autoHideScrollBar; set => AutoHideScrollBarValue = value; }

    bool IScrollableListWindow.IsScrollBarVisible => IsScrollBarVisible;
}
