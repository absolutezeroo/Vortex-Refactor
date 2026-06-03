// @see core/window/utils/tablet/TabletEventProcessor.as

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Graphics;

using WinController = Vortex.Core.Window.WindowController;

namespace Vortex.Core.Window.Utils.Tablet;

/// <summary>
/// Touch event processor. Extends MouseEventProcessor with state save/restore
/// pattern for tablet input.
/// </summary>
/// @see core/window/utils/tablet/TabletEventProcessor.as
public class TabletEventProcessor : MouseEventProcessor
{
    private readonly string _lastEventType = "";

    /// @see TabletEventProcessor.as::process
    public new static void ProcessEvents(EventProcessorState state, IEventQueue queue)
    {
        if (queue is GenericEventQueue { EventCount: 0 })
        {
            return;
        }

        IDesktopWindow? desktop = state.Desktop;
        WinController? hoveredWindow = state.CurrentHoveredWindow as WinController;
        WinController? lastClickTarget = state.LastClickTarget as WinController;
        WindowRenderer? renderer = state.Renderer;
        List<IInputEventTracker>? eventTrackers = state.EventTrackers;

        queue.Begin();
        queue.End();

        state.Desktop = desktop;
        state.CurrentHoveredWindow = hoveredWindow;
        state.LastClickTarget = lastClickTarget;
        state.Renderer = renderer;
        state.EventTrackers = eventTrackers;
    }
}
