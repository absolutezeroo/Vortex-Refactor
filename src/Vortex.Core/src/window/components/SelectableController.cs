// @see core/window/components/SelectableController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/SelectableController.as
public class SelectableController : InteractiveController, ISelectableWindow
{
    /// @see SelectableController.as::SelectableController (default)
    public SelectableController() : base() { }

    /// @see SelectableController.as::SelectableController (name + rect)
    public SelectableController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see SelectableController.as::SelectableController (full AS3 11-param signature)
    public SelectableController
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

    /// @see SelectableController.as::get selector
    public ISelectorWindow? SelectorParent
    {
        get
        {
            // @see SelectableController.as — traverse parent chain to find ISelectorWindow
            IWindow? current = parent;

            while (current != null)
            {
                if (current is ISelectorWindow selector)
                {
                    return selector;
                }

                current = current.parent;
            }

            return null;
        }
    }

    /// @see SelectableController.as::get isSelected
    public bool IsSelected
    {
        get => TestStateFlag(STATE_SELECTED);
        set => SetStateFlag(STATE_SELECTED, value);
    }

    /// @see ISelectableWindow — explicit interface
    IWindow? ISelectableWindow.Selector => SelectorParent as IWindow;

    /// @see SelectableController.as::select — routes events through Update()
    public virtual bool Select()
    {
        // @see SelectableController.as — idempotency guard
        if (GetStateFlag(STATE_SELECTED))
        {
            return true;
        }

        // @see SelectableController.as — fire WE_SELECT (cancelable) via update()
        WindowEvent evt = new(WindowEvent.WE_SELECT, this, null, true);
        Update(this, evt);

        if (evt.IsWindowOperationPrevented())
        {
            return false;
        }

        // @see SelectableController.as — set selected flag
        SetStateFlag(STATE_SELECTED, true);

        // @see SelectableController.as — fire WE_SELECTED via update()
        WindowEvent selectedEvt = new(WindowEvent.WE_SELECTED, this, null, false);
        Update(this, selectedEvt);

        // @see SelectableController.as — activate after selection
        Activate();

        return true;
    }

    /// @see SelectableController.as::unselect — routes events through Update()
    public virtual bool Unselect()
    {
        // @see SelectableController.as — idempotency guard
        if (!GetStateFlag(STATE_SELECTED))
        {
            return true;
        }

        // @see SelectableController.as — fire WE_UNSELECT (cancelable) via update()
        WindowEvent evt = new(WindowEvent.WE_UNSELECT, this, null, true);
        Update(this, evt);

        if (evt.IsWindowOperationPrevented())
        {
            return false;
        }

        // @see SelectableController.as — clear selected flag
        SetStateFlag(STATE_SELECTED, false);

        // @see SelectableController.as — fire WE_UNSELECTED via update()
        WindowEvent unselectedEvt = new(WindowEvent.WE_UNSELECTED, this, null, false);
        Update(this, unselectedEvt);

        return true;
    }


    /// @see SelectableController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type != WindowEvent.WE_ACTIVATED)
        {
            return base.Update(param1, param2);
        }

        // @see SelectableController.as — forward activation to selector as WE_CHILD_ACTIVATED
        ISelectorWindow? selectorParent = SelectorParent;

        if (selectorParent is not WindowController selectorController)
        {
            return base.Update(param1, param2);
        }

        WindowEvent childActivatedEvt = new(
            WindowEvent.WE_CHILD_ACTIVATED, selectorController, this, false
        );
        selectorController.NotifyEventListeners(childActivatedEvt);

        return base.Update(param1, param2);
    }
}
