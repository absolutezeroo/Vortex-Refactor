// @see core/window/components/TabButtonController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/TabButtonController.as
public class TabButtonController : SelectableController, ITabButtonWindow
{
    /// @see TabButtonController.as — tag constants
    public const string CONTENT_TAG = "TAB_BUTTON_CONTENT";
    public const string LABEL_TAG = "TAB_BUTTON_TITLE";
    public const string ICON_TAG = "TAB_BUTTON_ICON";

    /// @see TabButtonController.as::TabButtonController (default)
    public TabButtonController() : base() { }

    /// @see TabButtonController.as::TabButtonController (name + rect)
    public TabButtonController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see TabButtonController.as::TabButtonController (full AS3 11-param signature)
    /// @see TabButtonController.as — param4 |= 1 before super call
    public TabButtonController
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

    /// @see TabButtonController.as::set caption
    public override string caption
    {
        get => _caption;
        set
        {
            // @see TabButtonController.as — calls super.caption first, then propagates to child
            base.caption = value;

            IWindow? labelChild = FindChildByTag(LABEL_TAG);

            if (labelChild != null)
            {
                labelChild.caption = value;
            }
        }
    }

    /// @see TabButtonController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        // @see TabButtonController.as — on WE_CHILD_RESIZED, resize to accommodate,
        // then ALWAYS forward to super.update() (AS3 does not return early)
        if (param2.type == WindowEvent.WE_CHILD_RESIZED)
        {
            ResizeToAccommodateChildren(this);
        }

        return base.Update(param1, param2);
    }
}
