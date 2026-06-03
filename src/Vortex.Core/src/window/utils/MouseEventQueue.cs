// @see core/window/utils/MouseEventQueue.as

using Godot;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Mouse-specific event queue that tracks mouse position alongside buffered events.
/// Godot adaptation: receives InputEventMouse events pushed from WindowContext._Input().
/// </summary>
/// @see core/window/utils/MouseEventQueue.as
public class MouseEventQueue : GenericEventQueue
{
    /// @see MouseEventQueue.as::get mousePosition
    public Vector2 MousePosition { get; private set; } = Vector2.Zero;

    /// @see MouseEventQueue.as::mouseEventListener
    public override object? EventListener(params object?[] args)
    {
        return null;
    }

    /// <summary>
    /// Enqueue a mouse input event and update tracked position.
    /// Called from WindowContext's Godot _Input override.
    /// </summary>
    public void EnqueueMouseEvent(InputEvent evt)
    {
        if (evt is InputEventMouse mouseEvt)
        {
            MousePosition = mouseEvt.Position;
        }
        Enqueue(evt);
    }
}
