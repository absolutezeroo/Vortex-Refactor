// @see core/window/components/PasswordFieldController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/PasswordFieldController.as
/// Text field that displays input as masked characters.
public class PasswordFieldController : TextFieldController
{
    /// @see PasswordFieldController.as::PasswordFieldController (default)
    public PasswordFieldController() : base() { }

    /// @see PasswordFieldController.as::PasswordFieldController (name + rect)
    public PasswordFieldController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see PasswordFieldController.as::PasswordFieldController (full AS3 11-param signature)
    public PasswordFieldController
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
        // @see PasswordFieldController.as — set displayAsPassword = true
        DisplayAsPassword = true;
    }
}
