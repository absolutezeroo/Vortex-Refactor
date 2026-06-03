// @see core/window/components/ScrollBarLiftController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ScrollBarLiftController.as
/// Draggable thumb/lift for scroll bars. Calculates normalized offset on drag.
public class ScrollBarLiftController : InteractiveController
{
    private readonly ScrollBarController? _scrollBar;

    /// @see ScrollBarLiftController.as::ScrollBarLiftController (default)
    public ScrollBarLiftController() : base() { }

    /// @see ScrollBarLiftController.as::ScrollBarLiftController (name + rect)
    public ScrollBarLiftController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ScrollBarLiftController.as::ScrollBarLiftController (full AS3 11-param signature)
    public ScrollBarLiftController
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
    ) : base(param1, param2, param3, (uint)(param4 | 32 | 32768 | 257), param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see ScrollBarLiftController.as — walk up parent chain to find ScrollBarController
        IWindow? current = param7;

        while (current != null)
        {
            if (current is ScrollBarController scrollBar)
            {
                _scrollBar = scrollBar;
                break;
            }

            current = current.parent;
        }
    }

    /// @see ScrollBarLiftController.as::get scrollbarOffsetX
    public float ScrollbarOffsetX { get; private set; }

    /// @see ScrollBarLiftController.as::get scrollbarOffsetY
    public float ScrollbarOffsetY { get; private set; }

    /// @see ScrollBarLiftController.as::offset
    /// Called when lift is repositioned (e.g., by drag). Calculates normalized offset.
    public virtual void Offset(float deltaX, float deltaY)
    {
        // @see ScrollBarLiftController.as — apply position delta
        base.x += deltaX;
        base.y += deltaY;

        // @see ScrollBarLiftController.as — calculate normalized scroll offset
        if (parent != null)
        {
            float trackW = parent.width - width;
            float trackH = parent.height - height;

            ScrollbarOffsetX = trackW > 0 ? base.x / trackW : 0;
            ScrollbarOffsetY = trackH > 0 ? base.y / trackH : 0;
        }

        // @see ScrollBarLiftController.as — notify scrollbar of position change
        if (_scrollBar != null && parent != _scrollBar)
        {
            WindowEvent evt = WindowEvent.Allocate(WindowEvent.WE_CHILD_RELOCATED, this, null);
            _scrollBar.Update(this, evt);
        }
    }
}
