// @see core/window/components/TabContextController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/TabContextController.as
public class TabContextController : WindowController, ITabContextWindow
{
    private ISelectorWindow? _selector;
    private IWindow? _container;
    private readonly bool _useSelectorIterator = true;

    /// @see TabContextController.as::TabContextController (default)
    public TabContextController() : base() { }

    /// @see TabContextController.as::TabContextController (name + rect)
    public TabContextController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see TabContextController.as::TabContextController (full AS3 11-param signature)
    public TabContextController
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
        // @see TabContextController.as — propagate style and procedure to internal children
        List<IWindow> internalChildren = new();
        GroupChildrenWithTag(TAG_INTERNAL, internalChildren, -1);

        foreach (IWindow child in internalChildren)
        {
            child.style = _style;

            if (child is WindowController wc)
            {
                wc.procedure = SelectorEventProc;
            }
        }

        _useSelectorIterator = true;
    }

    /// @see TabContextController.as::get selector — uses findChildByTag("_SELECTOR")
    public ISelectorWindow? SelectorWindow
    {
        get
        {
            if (_selector != null)
            {
                return _selector;
            }

            // @see TabContextController.as — find by tag, cast to ISelectorListWindow
            IWindow? found = FindChildByTag("_SELECTOR");

            if (found is ISelectorWindow sel)
            {
                _selector = sel;

                // @see TabContextController.as — wire selector event proc
                if (found is WindowController wc)
                {
                    wc.procedure = SelectorEventProc;
                }
            }

            return _selector;
        }
    }

    /// @see TabContextController.as::get container — uses findChildByTag("_CONTENT")
    public IWindow? ContainerWindow
    {
        get
        {
            if (_container != null)
            {
                return _container;
            }

            _container = FindChildByTag("_CONTENT");

            return _container;
        }
    }

    /// @see TabContextController.as::get numTabItems
    public int NumTabItems => SelectorWindow?.NumSelectables ?? 0;

    /// @see TabContextController.as::addTabItem
    public ISelectableWindow? AddTabItem(ISelectableWindow param1)
    {
        return SelectorWindow?.AddSelectable(param1);
    }

    /// @see TabContextController.as::addTabItemAt
    public ISelectableWindow? AddTabItemAt(ISelectableWindow param1, int param2)
    {
        return SelectorWindow?.AddSelectableAt(param1, param2);
    }

    /// @see TabContextController.as::removeTabItem
    public ISelectableWindow? RemoveTabItem(ISelectableWindow param1)
    {
        return SelectorWindow?.RemoveSelectable(param1);
    }

    /// @see TabContextController.as::getTabItemAt
    public ISelectableWindow? GetTabItemAt(int param1)
    {
        return SelectorWindow?.GetSelectableAt(param1);
    }

    /// @see TabContextController.as::getTabItemByName
    public ISelectableWindow? GetTabItemByName(string param1)
    {
        return SelectorWindow?.GetSelectableByName(param1);
    }

    /// @see TabContextController.as::getTabItemByID
    public ISelectableWindow? GetTabItemByID(uint param1)
    {
        return SelectorWindow?.GetSelectableByID(param1);
    }

    /// @see TabContextController.as::getTabItemIndex
    public int GetTabItemIndex(ISelectableWindow param1)
    {
        return SelectorWindow?.GetSelectableIndex(param1) ?? -1;
    }

    /// @see TabContextController.as::selectorEventProc
    private void SelectorEventProc(WindowEvent param1, IWindow param2)
    {
        // @see TabContextController.as — forward WE_SELECTED events to this controller's listeners
        if (param1.type == WindowEvent.WE_SELECTED)
        {
            NotifyEventListeners(param1);
        }
    }

    // ITabContextWindow explicit property implementations

    ISelectorWindow? ITabContextWindow.Selector => SelectorWindow;

    IWindow? ITabContextWindow.Container => ContainerWindow;
}
