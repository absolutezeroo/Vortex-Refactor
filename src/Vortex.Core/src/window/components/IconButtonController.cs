// @see core/window/components/IconButtonController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IconButtonController.as
public class IconButtonController : InteractiveController, IIconButtonWindow
{
    /// @see IconButtonController.as::IconButtonController (default)
    public IconButtonController() : base() { }

    /// @see IconButtonController.as::IconButtonController (name + rect)
    public IconButtonController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see IconButtonController.as::IconButtonController (full AS3 11-param signature)
    public IconButtonController
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
}
