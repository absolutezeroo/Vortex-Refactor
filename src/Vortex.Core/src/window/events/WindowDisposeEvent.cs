// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowDisposeEvent.as

namespace Vortex.Core.Window.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowDisposeEvent.as
public sealed class WindowDisposeEvent : WindowEvent
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowDisposeEvent.as
    public const string WINDOW_DISPOSE_EVENT = "WINDOW_DISPOSE_EVENT";

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowDisposeEvent.as
    public WindowDisposeEvent(IWindow? param1)
        : base(WINDOW_DISPOSE_EVENT, param1, null)
    {
    }
}
