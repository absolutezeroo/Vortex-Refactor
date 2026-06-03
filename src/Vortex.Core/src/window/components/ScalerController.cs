// @see core/window/components/ScalerController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// <summary>
/// Controller for scaler/resize handle windows.
/// AS3 ScalerController is a pass-through to InteractiveController;
/// resize drag is initiated by mouse down via WindowMouseScaler service.
/// </summary>
/// @see core/window/components/ScalerController.as
public class ScalerController : InteractiveController, IScalerWindow
{
    /// @see ScalerController.as::ScalerController (default)
    public ScalerController() : base() { }

    /// @see ScalerController.as::ScalerController (name + rect)
    public ScalerController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ScalerController.as::ScalerController (full AS3 11-param signature)
    public ScalerController
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
