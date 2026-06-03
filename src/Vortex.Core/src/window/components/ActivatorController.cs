// @see WIN63-202407091256-704579380-Source-main/core/window/components/ActivatorController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see WIN63-202407091256-704579380-Source-main/core/window/components/ActivatorController.as
public class ActivatorController : WindowController
{
    protected IWindow? _activeChild;

    /// @see ActivatorController.as::ActivatorController
    public ActivatorController() : base() { }

    /// @see ActivatorController.as::ActivatorController
    public ActivatorController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ActivatorController.as::ActivatorController (full AS3 11-param signature)
    public ActivatorController
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

    /// @see ActivatorController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type != WindowEvent.WE_CHILD_ACTIVATED)
        {
            return param2.type == WindowEvent.WE_PARENT_ACTIVATED || base.Update(param1, param2);
        }

        SetActiveChild(param1);

        return true;
    }

    /// @see ActivatorController.as::getActiveChild
    public IWindow? GetActiveChild()
    {
        return _activeChild;
    }

    /// @see ActivatorController.as::setActiveChild
    public IWindow? SetActiveChild(IWindow? param1)
    {
        // Walk up to find direct child of this container
        while (param1 != null && param1.parent != this)
        {
            param1 = param1.parent!;

            if (param1 == null)
            {
                throw new Exception("Window passed to activator is not a child!");
            }
        }

        IWindow? previous = _activeChild;

        if (_activeChild == param1)
        {
            return previous;
        }

        if (_activeChild is { disposed: false })
        {
            _activeChild.Deactivate();
        }

        _activeChild = param1;

        // Bring to front (topmost z-order)
        if (GetChildIndex(param1) != numChildren - 1)
        {
            SetChildIndex(param1, numChildren - 1);
        }

        return previous;
    }
}
