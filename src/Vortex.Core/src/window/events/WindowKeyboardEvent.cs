// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as

namespace Vortex.Core.Window.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as
public sealed class WindowKeyboardEvent : WindowEvent
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as::const_814
    public const string KEY_UP = "WKE_KEY_UP";

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as::const_741
    public const string KEY_DOWN = "WKE_KEY_DOWN";

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as
    public WindowKeyboardEvent(string param1, IWindow? param2, IWindow? param3, uint param4, uint param5)
        : base(param1, param2, param3, true)
    {
        keyCode = param4;
        charCode = param5;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as
    public uint keyCode { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as
    public uint charCode { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowKeyboardEvent.as::allocate
    public static WindowKeyboardEvent Allocate(string param1, IWindow? param2, IWindow? param3, uint param4, uint param5)
    {
        return new WindowKeyboardEvent(param1, param2, param3, param4, param5);
    }
}
