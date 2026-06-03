// @see core/window/components/SelectorController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/SelectorController.as
public class SelectorController : InteractiveController, ISelectorWindow
{
    private ISelectableWindow? _selected;

    /// @see SelectorController.as — controls whether selected child is brought to front
    protected bool _reorderOnSelect = true;

    /// @see SelectorController.as::SelectorController (default)
    public SelectorController() : base() { }

    /// @see SelectorController.as::SelectorController (name + rect)
    public SelectorController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see SelectorController.as::SelectorController (full AS3 11-param signature)
    public SelectorController
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

    /// @see SelectorController.as::get iterator
    public virtual object? Iterator()
    {
        return new SelectorIterator(this);
    }

    /// @see SelectorController.as::get numSelectables
    public int NumSelectables => numChildren;

    /// @see SelectorController.as::getSelected
    public ISelectableWindow? GetSelected()
    {
        return _selected;
    }

    /// @see SelectorController.as::setSelected
    public void SetSelected(ISelectableWindow? param1)
    {
        if (param1 == _selected)
        {
            return;
        }

        ISelectableWindow? previous = _selected;

        // @see SelectorController.as — unselect previous
        if (_selected != null)
        {
            _selected.Unselect();

            // If unselect was prevented, abort
            if (_selected.IsSelected)
            {
                return;
            }
        }

        _selected = param1;

        if (param1 == null)
        {
            return;
        }

        param1.Select();

        // @see SelectorController.as — bring to front if _reorderOnSelect
        if (_reorderOnSelect && param1 is IWindow selectableWindow)
        {
            int childIndex = GetChildIndex(selectableWindow);

            if (childIndex >= 0 && childIndex < numChildren - 1)
            {
                SetChildIndex(selectableWindow, numChildren - 1);
            }
        }

        // If select was prevented, revert
        if (param1.IsSelected)
        {
            return;
        }

        _selected = previous;

        previous?.Select();
    }

    /// @see SelectorController.as::addSelectable
    public ISelectableWindow? AddSelectable(ISelectableWindow param1)
    {
        if (param1 is IWindow window)
        {
            AddChild(window);
        }

        return param1;
    }

    /// @see SelectorController.as::addSelectableAt
    public ISelectableWindow? AddSelectableAt(ISelectableWindow param1, int param2)
    {
        if (param1 is IWindow window)
        {
            AddChildAt(window, param2);
        }

        return param1;
    }

    /// @see SelectorController.as::getSelectableAt
    public ISelectableWindow? GetSelectableAt(int param1)
    {
        return GetChildAt(param1) as ISelectableWindow;
    }

    /// @see SelectorController.as::getSelectableByID
    public ISelectableWindow? GetSelectableByID(uint param1)
    {
        return GetChildByID((int)param1) as ISelectableWindow;
    }

    /// @see SelectorController.as::getSelectableByTag
    public ISelectableWindow? GetSelectableByTag(string param1)
    {
        return FindChildByTag(param1) as ISelectableWindow;
    }

    /// @see SelectorController.as::getSelectableByName
    public ISelectableWindow? GetSelectableByName(string param1)
    {
        return FindChildByName(param1) as ISelectableWindow;
    }

    /// @see SelectorController.as::getSelectableIndex
    public int GetSelectableIndex(ISelectableWindow param1)
    {
        if (param1 is IWindow window)
        {
            return GetChildIndex(window);
        }

        return -1;
    }

    /// @see SelectorController.as::removeSelectable
    public ISelectableWindow? RemoveSelectable(ISelectableWindow param1)
    {
        if (param1 is not IWindow window)
        {
            return null;
        }

        int index = GetChildIndex(window);

        if (index < 0)
        {
            return null;
        }

        // @see SelectorController.as — if removing selected, fallback to index 0 or 1
        if (param1 == _selected)
        {
            _selected = null;

            if (numChildren > 1)
            {
                int fallbackIndex = index == 0 ? 1 : 0;

                if (GetChildAt(fallbackIndex) is ISelectableWindow nextChild)
                {
                    SetSelected(nextChild);
                }
            }
        }

        RemoveChild(window);

        return param1;
    }

    /// @see SelectorController.as::update — check param1 (event source) for ISelectableWindow
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type != WindowEvent.WE_CHILD_ACTIVATED || param1 is not ISelectableWindow selectable)
        {
            return base.Update(param1, param2);
        }

        SetSelected(selectable);

        return true;
    }

}
