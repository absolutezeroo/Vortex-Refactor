// @see core/window/components/ScrollableItemGridWindow.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ScrollableItemGridWindow.as
/// Thin wrapper around ItemGridController + ScrollBarController, found via layout XML tags.
public class ScrollableItemGridWindow : WindowController, IScrollableGridWindow
{
    private ItemGridController? _cachedItemGrid;
    private ScrollBarController? _cachedScrollBar;
    private bool _autoHideScrollBar = true;

    /// @see ScrollableItemGridWindow.as::ScrollableItemGridWindow (default)
    public ScrollableItemGridWindow() : base() { }

    /// @see ScrollableItemGridWindow.as::ScrollableItemGridWindow (name + rect)
    public ScrollableItemGridWindow(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ScrollableItemGridWindow.as::ScrollableItemGridWindow (full AS3 11-param signature)
    public ScrollableItemGridWindow
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
        // @see ScrollableItemGridWindow.as — wire scrollbar to item grid
        ScrollBarController? scrollBar = GetScrollBar();
        ItemGridController? itemGrid = GetItemGrid();

        if (scrollBar != null && itemGrid is IScrollableWindow scrollable)
        {
            scrollBar.ScrollableTarget = scrollable;
        }
    }

    /// @see ScrollableItemGridWindow.as::get _itemGrid
    protected ItemGridController? GetItemGrid()
    {
        if (_cachedItemGrid != null)
        {
            return _cachedItemGrid;
        }

        _cachedItemGrid = FindChildByTag("_ITEMGRID") as ItemGridController;

        return _cachedItemGrid;
    }

    /// @see ScrollableItemGridWindow.as::get _scrollBar
    protected ScrollBarController? GetScrollBar()
    {
        if (_cachedScrollBar != null)
        {
            return _cachedScrollBar;
        }

        _cachedScrollBar = FindChildByTag("_SCROLLBAR") as ScrollBarController;

        return _cachedScrollBar;
    }

    /// @see ScrollableItemGridWindow.as::get autoHideScrollBar
    public bool AutoHideScrollBarValue
    {
        get => _autoHideScrollBar;
        set
        {
            _autoHideScrollBar = value;
            UpdateScrollBarVisibility();
        }
    }

    /// @see ScrollableItemGridWindow.as::get numGridItems
    public int NumGridItems => GetItemGrid()?.NumGridItems ?? 0;

    /// @see ScrollableItemGridWindow.as::get numColumns
    public int NumColumns => GetItemGrid()?.NumColumns ?? 0;

    /// @see ScrollableItemGridWindow.as::get numRows
    public int NumRows => GetItemGrid()?.NumRows ?? 0;

    /// @see ScrollableItemGridWindow.as::get iterator
    public object? GridIterator => GetItemGrid()?.Iterator();

    public IWindow? AddGridItem(IWindow item)
    {
        return GetItemGrid()?.AddGridItem(item);
    }

    public IWindow? RemoveGridItem(IWindow item)
    {
        return GetItemGrid()?.RemoveGridItem(item);
    }

    public IWindow? GetGridItemAt(int index)
    {
        return GetItemGrid()?.GetGridItemAt(index);
    }

    public int GetGridItemIndex(IWindow item)
    {
        return GetItemGrid()?.GetGridItemIndex(item) ?? -1;
    }

    public void RemoveGridItems()
    {
        GetItemGrid()?.RemoveGridItems();
    }

    public void DestroyGridItems()
    {
        GetItemGrid()?.DestroyGridItems();
    }

    public void RebuildGridStructure()
    {
        GetItemGrid()?.RebuildGridStructure();
    }

    /// @see ScrollableItemGridWindow.as::hideScrollBar
    public void HideScrollBar()
    {
        ScrollBarController? sb = GetScrollBar();

        if (sb != null)
        {
            sb.visible = false;
        }
    }

    /// @see ScrollableItemGridWindow.as::showScrollBar
    public void ShowScrollBar()
    {
        ScrollBarController? sb = GetScrollBar();

        if (sb != null)
        {
            sb.visible = true;
        }
    }

    /// @see ScrollableItemGridWindow.as::updateScrollBarVisibility
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

    /// @see ScrollableItemGridWindow.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        _cachedItemGrid = null;
        _cachedScrollBar = null;

        return base.Destroy();
    }

    bool IScrollableGridWindow.AutoHideScrollBar { get => _autoHideScrollBar; set => AutoHideScrollBarValue = value; }
}
