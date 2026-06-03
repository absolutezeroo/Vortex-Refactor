// @see core/window/components/HeaderController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/HeaderController.as
public class HeaderController : ContainerController
{
    /// @see HeaderController.as::HeaderController (default)
    public HeaderController() : base() { }

    /// @see HeaderController.as::HeaderController (name + rect)
    public HeaderController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see HeaderController.as::HeaderController (full AS3 11-param signature)
    /// @see HeaderController.as — param4 |= 1 before super call
    public HeaderController
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

    /// @see HeaderController.as::get title — NOT cached per AS3
    public IWindow? title => FindChildByTag("_TITLE");

    /// @see HeaderController.as::get controls — NOT cached per AS3
    public IWindow? controls => FindChildByTag("_CONTROLS");

    /// @see HeaderController.as::set caption
    /// AS3 calls super.caption first (triggers invalidation), then sets title.text
    public override string caption
    {
        get => _caption;
        set
        {
            base.caption = value;

            try
            {
                IWindow? titleWindow = title;
                if (titleWindow != null)
                {
                    titleWindow.caption = value;
                }
            }
            catch
            {
                // @see HeaderController.as — try/catch around title.text set
            }
        }
    }

    /// @see HeaderController.as::set color — override, depth -1 (unlimited)
    public override uint color
    {
        get => base.color;
        set
        {
            base.color = value;

            List<IWindow> colorizeChildren = new();
            GroupChildrenWithTag(TAG_COLORIZE, colorizeChildren, -1);

            foreach (IWindow child in colorizeChildren)
            {
                child.color = value;
            }
        }
    }
}
