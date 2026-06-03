// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as

namespace Vortex.Core.Window.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as
public sealed class WindowTouchEvent : WindowEvent
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as::WINDOW_EVENT_TOUCH_BEGIN
    public const string WINDOW_EVENT_TOUCH_BEGIN = "WTE_BEGIN";

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as::WINDOW_EVENT_TOUCH_MOVE
    public const string WINDOW_EVENT_TOUCH_MOVE = "WTE_MOVE";

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as
    public WindowTouchEvent(string param1, IWindow? param2, IWindow? param3, float param4, float param5, float param6, float param7)
        : base(param1, param2, param3, true)
    {
        localX = param4;
        localY = param5;
        stageX = param6;
        stageY = param7;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as
    public float localX { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as
    public float localY { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as
    public float stageX { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as
    public float stageY { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/window/events/WindowTouchEvent.as::allocate
    public static WindowTouchEvent Allocate
    (
        string param1,
        IWindow? param2,
        IWindow? param3,
        float param4,
        float param5,
        float param6,
        float param7
    )
    {
        return new WindowTouchEvent(param1, param2, param3, param4, param5, param6, param7);
    }
}
