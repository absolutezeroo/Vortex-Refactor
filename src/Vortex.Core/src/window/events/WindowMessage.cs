// @see core/window/events/WindowMessage.as

namespace Vortex.Core.Window.Events;

/// <summary>
/// Window message event carrying a string message payload.
/// Uses object pooling via Allocate/Clone pattern matching WindowMouseEvent.
/// </summary>
/// @see core/window/events/WindowMessage.as
public class WindowMessage : WindowEvent
{
    public const string WINDOW_EVENT_MESSAGE = "WE_MESSAGE";

    public string message { get; set; } = "";

    /// @see WindowMessage.as::WindowMessage
    public WindowMessage() : base(WINDOW_EVENT_MESSAGE, null, null) { }

    /// @see WindowMessage.as::allocate
    public static WindowEvent Allocate(string message, IWindow? window, IWindow? related, bool cancelable = false)
    {
        WindowMessage evt = new()
        {
            message = message,
            window = window,
            related = related,
            cancelable = cancelable,
        };
        return evt;
    }

    /// @see WindowMessage.as::clone
    public override WindowEvent Clone()
    {
        return Allocate(message, window, related, cancelable);
    }

    public override string ToString()
    {
        return $"WindowMessage {{ type: {type} message: {message} cancelable: {cancelable} window: {window?.name} }}";
    }
}
