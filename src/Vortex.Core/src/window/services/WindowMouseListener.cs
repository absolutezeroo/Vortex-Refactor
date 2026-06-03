// @see core/window/services/WindowMouseListener.as

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Services;

/// <summary>
/// Mouse listener service that filters events by type and area (inside/outside window).
/// Dispatches matching events to the window's Update() method.
/// </summary>
/// @see core/window/services/WindowMouseListener.as
public class WindowMouseListener : WindowMouseOperator
{
    /// @see WindowMouseListener.as::WindowMouseListener
    public WindowMouseListener() : base()
    {
        AreaLimit = 0;
    }

    /// @see WindowMouseListener.as::get eventTypes
    public List<string> EventTypes { get; } = new();

    /// @see WindowMouseListener.as::get/set areaLimit
    public uint AreaLimit { get; set; }

    /// @see WindowMouseListener.as::end
    public override IWindow? End(IWindow? window)
    {
        EventTypes.Clear();
        return base.End(window);
    }

    /// <summary>
    /// Handles a WindowMouseEvent by checking if its type is in the filter list
    /// and if the mouse position passes the area constraint.
    /// Godot adaptation: called directly from MouseEventProcessor instead of Flash event dispatch.
    /// </summary>
    /// @see WindowMouseListener.as::handler
    public void HandleWindowMouseEvent(WindowMouseEvent mouseEvent)
    {
        if (!_active || _window == null || _window.disposed)
        {
            return;
        }

        if (EventTypes.Contains(mouseEvent.type))
        {
            bool hitTest = _window.HitTestGlobalPoint(new Vector2(mouseEvent.stageX, mouseEvent.stageY));

            // areaLimit: 0 = all events, 1 = inside window only, 3 = outside window only
            if (AreaLimit == 1 && !hitTest)
            {
                return;
            }
            if (AreaLimit == 3 && hitTest)
            {
                return;
            }

            _window.Update(null, mouseEvent);
        }
    }

    /// @see WindowMouseListener.as::operate
    public override void Operate(int mouseX, int mouseY)
    {
        // No-op — listener doesn't drag/scale
    }
}
