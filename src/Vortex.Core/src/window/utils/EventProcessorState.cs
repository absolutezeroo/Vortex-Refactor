// @see core/window/utils/EventProcessorState.as

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Mutable state container for event processing. Holds references to the renderer,
/// desktop, current hover/focus/click targets, and registered event trackers.
/// </summary>
/// @see core/window/utils/EventProcessorState.as
public class EventProcessorState
{
    public WindowRenderer? Renderer { get; set; }
    public IDesktopWindow? Desktop { get; set; }
    public IWindow? CurrentHoveredWindow { get; set; }
    public IWindow? LastClickTarget { get; set; }
    public IWindow? LastMouseDownTarget { get; set; }
    public IWindow? FocusedControl { get; set; }
    public List<IInputEventTracker>? EventTrackers { get; set; }

    public EventProcessorState() { }

    /// @see EventProcessorState.as::EventProcessorState
    public EventProcessorState
    (
        WindowRenderer? renderer,
        IDesktopWindow? desktop,
        IWindow? currentHoveredWindow,
        IWindow? lastClickTarget,
        IWindow? lastMouseDownTarget,
        IWindow? focusedControl,
        List<IInputEventTracker>? eventTrackers
    )
    {
        Renderer = renderer;
        Desktop = desktop;
        CurrentHoveredWindow = currentHoveredWindow;
        LastClickTarget = lastClickTarget;
        LastMouseDownTarget = lastMouseDownTarget;
        FocusedControl = focusedControl;
        EventTrackers = eventTrackers;
    }
}
