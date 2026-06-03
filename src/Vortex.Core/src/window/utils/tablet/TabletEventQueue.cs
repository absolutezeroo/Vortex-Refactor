// @see core/window/utils/tablet/TabletEventQueue.as

using Godot;

namespace Vortex.Core.Window.Utils.Tablet;

/// <summary>
/// Touch-aware event queue extending GenericEventQueue with touch position tracking.
/// </summary>
/// @see core/window/utils/tablet/TabletEventQueue.as
public class TabletEventQueue : GenericEventQueue
{
    /// @see TabletEventQueue.as::TabletEventQueue
    public TabletEventQueue()
    {
        TouchPosition = Vector2.Zero;
    }

    /// @see TabletEventQueue.as::get touchPosition
    public Vector2 TouchPosition { get; }

    /// @see TabletEventQueue.as::dispose
    public override void DisposeQueue()
    {
        if (!_disposed)
        {
            base.DisposeQueue();
        }
    }
}
