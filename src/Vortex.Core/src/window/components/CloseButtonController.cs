// @see core/window/components/CloseButtonController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// <summary>
/// Controller for close button windows.
/// AS3 CloseButtonController is a pass-through to InteractiveController.
/// Close behavior is handled via WE_CLOSE event dispatch in the parent's procedure.
/// </summary>
/// @see core/window/components/CloseButtonController.as
public class CloseButtonController : InteractiveController, IClass3542
{
    /// @see CloseButtonController.as::CloseButtonController (default)
    public CloseButtonController() : base() { }

    /// @see CloseButtonController.as::CloseButtonController (name + rect)
    public CloseButtonController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see CloseButtonController.as::CloseButtonController (full AS3 11-param signature)
    public CloseButtonController
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
