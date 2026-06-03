// @see core/window/components/DropMenuItemController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/DropMenuItemController.as
public class DropMenuItemController : ButtonController
{
    /// @see DropMenuItemController.as::DropMenuItemController (default)
    public DropMenuItemController() : base() { }

    /// @see DropMenuItemController.as::DropMenuItemController (name + rect)
    public DropMenuItemController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see DropMenuItemController.as::DropMenuItemController (full AS3 11-param signature)
    public DropMenuItemController
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

    /// @see DropMenuItemController.as::get menu
    /// Walks parent chain to find DropMenuController
    public DropMenuController? Menu
    {
        get
        {
            IWindow? current = parent;

            while (current != null)
            {
                if (current is DropMenuController menu)
                {
                    return menu;
                }

                current = current.parent;
            }

            return null;
        }
    }

    /// @see DropMenuItemController.as::get value
    public IWindow Value => this;
}
