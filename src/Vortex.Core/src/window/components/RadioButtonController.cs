// @see core/window/components/RadioButtonController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/RadioButtonController.as
public class RadioButtonController : SelectableController, IRadioButtonWindow
{
    /// @see RadioButtonController.as — text field child name
    private const string TEXT_FIELD_NAME = "_CAPTION_TEXT";

    /// @see RadioButtonController.as::RadioButtonController (default)
    public RadioButtonController() : base() { }

    /// @see RadioButtonController.as::RadioButtonController (name + rect)
    public RadioButtonController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see RadioButtonController.as::RadioButtonController (full AS3 11-param signature)
    /// @see RadioButtonController.as — param4 |= 1 before super call
    public RadioButtonController
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
    ) : base(param1, param2, param3, param4 | 1, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }

    /// @see RadioButtonController.as::set caption
    public override string caption
    {
        get => _caption;
        set
        {
            _caption = value;

            IWindow? textField = GetChildByName(TEXT_FIELD_NAME);

            if (textField != null)
            {
                textField.caption = value;
            }
        }
    }

    /// @see RadioButtonController.as::setRectangle
    /// Godot adaptation: AS3 setRectangle(x,y,w,h) → override width setter to cascade to caption child
    public override float width
    {
        get => base.width;
        set
        {
            base.width = value;

            // @see RadioButtonController.as — resize caption text to match new width
            IWindow? textField = GetChildByName(TEXT_FIELD_NAME);

            if (textField != null)
            {
                textField.width = value;
            }
        }
    }
}
