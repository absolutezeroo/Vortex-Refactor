// @see core/window/components/DropListItemController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/DropListItemController.as
public class DropListItemController : ContainerButtonController
{
    /// @see DropListItemController.as::DropListItemController (default)
    public DropListItemController() : base() { }

    /// @see DropListItemController.as::DropListItemController (name + rect)
    public DropListItemController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see DropListItemController.as::DropListItemController (full AS3 11-param signature)
    public DropListItemController
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

    /// @see DropListItemController.as::get menu
    public DropBaseController? Menu
    {
        get
        {
            IWindow? current = parent;

            while (current != null)
            {
                if (current is DropBaseController menu)
                {
                    return menu;
                }

                current = current.parent;
            }

            return null;
        }
    }

    /// @see DropListItemController.as::get value
    public IWindow? Value
    {
        get => numChildren > 0 ? GetChildAt(0) : null;
        set
        {
            if (numChildren > 0)
            {
                RemoveChildAt(0);
            }

            if (value != null)
            {
                AddChild(value);
            }
        }
    }
}
