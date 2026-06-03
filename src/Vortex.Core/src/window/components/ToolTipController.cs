// @see core/window/components/ToolTipController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ToolTipController.as
public class ToolTipController : ButtonController, IToolTipWindow
{
    /// @see ToolTipController.as::ToolTipController (default)
    public ToolTipController() : base() { }

    /// @see ToolTipController.as::ToolTipController (name + rect)
    public ToolTipController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ToolTipController.as::ToolTipController (full AS3 11-param signature)
    /// @see ToolTipController.as — param4 |= 131072 before super call
    public ToolTipController
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
    ) : base(param1, param2, param3, param4 | 131072, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }
}
