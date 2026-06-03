// @see core/window/events/WindowLinkEvent.as

namespace Vortex.Core.Window.Events;

/// <summary>
/// Window link event carrying a URL/link string payload.
/// Uses Allocate/Clone pattern matching WindowMouseEvent.
/// </summary>
/// @see core/window/events/WindowLinkEvent.as
public class WindowLinkEvent : WindowEvent
{
    public const string WINDOW_EVENT_LINK = "WE_LINK";

    /// @see WindowLinkEvent.as::WindowLinkEvent
    public WindowLinkEvent() : base(WINDOW_EVENT_LINK, null, null) { }

    /// @see WindowLinkEvent.as::get link
    public string link { get; private set; } = "";

    /// @see WindowLinkEvent.as::allocate
    public static WindowEvent Allocate(string link, IWindow? window, IWindow? related)
    {
        WindowLinkEvent evt = new()
        {
            link = link,
            window = window,
            related = related,
        };
        return evt;
    }

    /// @see WindowLinkEvent.as::clone
    public override WindowEvent Clone()
    {
        return Allocate(link, window, related);
    }

    public override string ToString()
    {
        return $"WindowLinkEvent {{ type: {type} link: {link} cancelable: {cancelable} window: {window?.name} }}";
    }
}
