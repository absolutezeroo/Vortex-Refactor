// @see core/window/components/BorderController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/BorderController.as
public class BorderController : ContainerController
{
    /// @see BorderController.as::BorderController (default)
    public BorderController() : base() { }

    /// @see BorderController.as::BorderController (name + rect)
    public BorderController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see BorderController.as::BorderController (full AS3 11-param signature)
    public BorderController
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
