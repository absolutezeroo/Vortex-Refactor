// @see core/window/components/ButtonController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ButtonController.as
public class ButtonController : InteractiveController
{
    /// @see ButtonController.as::ButtonController (default)
    public ButtonController() : base() { }

    /// @see ButtonController.as::ButtonController (name + rect)
    public ButtonController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ButtonController.as::ButtonController (full AS3 11-param signature)
    /// @see ButtonController.as — param4 |= 131072 before super call
    public ButtonController
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

    /// @see ButtonController.as::set caption — routes through base for localization + invalidation
    public override string caption
    {
        get => _caption;
        set
        {
            base.caption = value;

            IWindow? btnText = GetChildByName("_BTN_TEXT");

            if (btnText != null)
            {
                btnText.caption = value;
            }
        }
    }

    /// @see ButtonController.as::set blend
    public new float blend
    {
        get => base.blend;
        set
        {
            base.blend = value;

            IWindow? btnText = GetChildByName("_BTN_TEXT");

            if (btnText == null)
            {
                return;
            }

            // @see ButtonController.as — disabled state (flag 32) halves the text blend
            if (TestStateFlag(STATE_DISABLED))
            {
                btnText.blend = value * 0.5f;
            }
            else
            {
                btnText.blend = value;
            }
        }
    }

    /// @see ButtonController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        switch (param2.type)
        {
            case WindowEvent.WE_CHILD_RESIZED:
                // @see ButtonController.as — reset width to 0 on child resize
                width = 0;

                return base.Update(param1, param2);

            case WindowEvent.WE_ENABLED:
                {
                    // @see ButtonController.as — AS3 adds 0.5 to child's current blend
                    IWindow? btnText = GetChildByName("_BTN_TEXT");

                    if (btnText != null)
                    {
                        btnText.blend = btnText.blend + 0.5f;
                    }

                    return base.Update(param1, param2);
                }

            case WindowEvent.WE_DISABLED:
                {
                    // @see ButtonController.as — AS3 subtracts 0.5 from child's current blend
                    IWindow? btnText = GetChildByName("_BTN_TEXT");

                    if (btnText != null)
                    {
                        btnText.blend = btnText.blend - 0.5f;
                    }

                    return base.Update(param1, param2);
                }
        }

        return base.Update(param1, param2);
    }
}
