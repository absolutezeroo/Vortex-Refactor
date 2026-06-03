// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/events/WindowMouseEvent.as

namespace Vortex.Core.Window.Events;

/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/events/WindowMouseEvent.as
public sealed class WindowMouseEvent : WindowEvent
{
    public const string CLICK = "WME_CLICK";
    public const string DOUBLE_CLICK = "WME_DOUBLE_CLICK";
    public const string DOWN = "WME_DOWN";
    public const string MIDDLE_CLICK = "WME_MIDDLE_CLICK";
    public const string MIDDLE_DOWN = "WME_MIDDLE_DOWN";
    public const string MIDDLE_UP = "WME_MIDDLE_UP";
    public const string MOVE = "WME_MOVE";
    public const string OUT = "WME_OUT";
    public const string OVER = "WME_OVER";
    public const string UP = "WME_UP";
    public const string UP_OUTSIDE = "WME_UP_OUTSIDE";
    public const string WHEEL = "WME_WHEEL";
    public const string RIGHT_CLICK = "WME_RIGHT_CLICK";
    public const string RIGHT_DOWN = "WME_RIGHT_DOWN";
    public const string RIGHT_UP = "WME_RIGHT_UP";
    public const string ROLL_OUT = "WME_ROLL_OUT";
    public const string ROLL_OVER = "WME_ROLL_OVER";
    public const string HOVERING = "WME_HOVERING";
    public const string CLICK_AWAY = "WME_CLICK_AWAY";

    /// @see WindowMouseEvent.as::WindowMouseEvent
    public WindowMouseEvent
    (
        string param1,
        IWindow? param2,
        IWindow? param3,
        float param4,
        float param5,
        float param6,
        float param7,
        bool param8 = false,
        bool param9 = false,
        bool param10 = false,
        bool param11 = false,
        int param12 = 0
    )
        : base(param1, param2, param3, true)
    {
        localX = param4;
        localY = param5;
        stageX = param6;
        stageY = param7;
        altKey = param8;
        ctrlKey = param9;
        shiftKey = param10;
        buttonDown = param11;
        delta = param12;
    }

    public int delta { get; }

    public float localX { get; }

    public float localY { get; }

    public float stageX { get; }

    public float stageY { get; }

    public bool altKey { get; }

    public bool ctrlKey { get; }

    public bool shiftKey { get; }

    public bool buttonDown { get; }

    /// @see WindowMouseEvent.as::allocate
    public static WindowMouseEvent Allocate
    (
        string param1,
        IWindow? param2,
        IWindow? param3,
        float param4,
        float param5,
        float param6,
        float param7,
        bool param8 = false,
        bool param9 = false,
        bool param10 = false,
        bool param11 = false,
        int param12 = 0
    )
    {
        return new WindowMouseEvent(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12);
    }

    /// @see WindowMouseEvent.as::clone
    public override WindowEvent Clone()
    {
        return new WindowMouseEvent(
            type, window, related, localX, localY, stageX, stageY, altKey, ctrlKey, shiftKey, buttonDown, delta
        );
    }

    public override string ToString()
    {
        return $"WindowMouseEvent {{ type: {type}, local: ({localX},{localY}), stage: ({stageX},{stageY}), delta: {delta} }}";
    }
}
