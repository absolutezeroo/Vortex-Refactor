// @see core/window/components/CheckBoxController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/CheckBoxController.as
public class CheckBoxController : SelectableController
{
    /// @see CheckBoxController.as — text field child name
    private const string TEXT_FIELD_NAME = "_CAPTION_TEXT";

    /// @see CheckBoxController.as::CheckBoxController (default)
    public CheckBoxController() : base() { }

    /// @see CheckBoxController.as::CheckBoxController (name + rect)
    public CheckBoxController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see CheckBoxController.as::CheckBoxController (full AS3 11-param signature)
    public CheckBoxController
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

    /// @see CheckBoxController.as::set caption
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

    /// @see CheckBoxController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param2.type == WindowMouseEvent.UP)
        {
            // @see CheckBoxController.as — toggle selection on mouse up
            if (IsSelected)
            {
                Unselect();
            }
            else
            {
                Select();
            }

            return true;
        }

        return base.Update(param1, param2);
    }
}
