// @see core/window/components/SelectableButtonController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/SelectableButtonController.as
/// Note: Extends ButtonController (not SelectableController) but implements ISelectableWindow
public class SelectableButtonController : ButtonController, ISelectableWindow
{
    /// @see SelectableButtonController.as::SelectableButtonController (default)
    public SelectableButtonController() : base() { }

    /// @see SelectableButtonController.as::SelectableButtonController (name + rect)
    public SelectableButtonController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see SelectableButtonController.as::SelectableButtonController (full AS3 11-param signature)
    public SelectableButtonController
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

    /// @see SelectableButtonController.as::get selector
    public ISelectorWindow? SelectorParent
    {
        get
        {
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

    /// @see SelectableButtonController.as::get isSelected
    public bool IsSelected
    {
        get => TestStateFlag(STATE_SELECTED);
        set => SetStateFlag(STATE_SELECTED, value);
    }

    /// @see ISelectableWindow — explicit interface
    IWindow? ISelectableWindow.Selector => SelectorParent as IWindow;

    /// @see SelectableButtonController.as::select
    /// Note: Unlike SelectableController, does NOT call Activate() after selection
    public virtual bool Select()
    {
        WindowEvent evt = new(WindowEvent.WE_SELECT, this, null, true);

        NotifyEventListeners(evt);

        if (evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_SELECTED, true);

        WindowEvent selectedEvt = new(WindowEvent.WE_SELECTED, this, null, false);

        NotifyEventListeners(selectedEvt);

        return true;
    }

    /// @see SelectableButtonController.as::unselect
    public virtual bool Unselect()
    {
        WindowEvent evt = new(WindowEvent.WE_UNSELECT, this, null, true);

        NotifyEventListeners(evt);

        if (evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_SELECTED, false);

        WindowEvent unselectedEvt = new(WindowEvent.WE_UNSELECTED, this, null, false);

        NotifyEventListeners(unselectedEvt);

        return true;
    }


    /// @see SelectableButtonController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type != WindowEvent.WE_ACTIVATED)
        {
            return base.Update(param1, param2);
        }

        ISelectorWindow? selectorParent = SelectorParent;

        if (selectorParent is not WindowController selectorController)
        {
            return true;
        }

        WindowEvent childActivatedEvt = new(
            WindowEvent.WE_CHILD_ACTIVATED, selectorController, this, false
        );

        selectorController.NotifyEventListeners(childActivatedEvt);

        return true;

    }
}
