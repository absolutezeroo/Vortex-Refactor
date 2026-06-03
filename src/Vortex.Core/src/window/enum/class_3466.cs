// @see core/window/enum/class_3466.as
// @see PRODUCTION: core/window/enum/WindowState.as

namespace Vortex.Core.Window.Enum;

/// <summary>
/// Window state bit flags.
/// </summary>
/// @see core/window/enum/class_3466.as
public static class Class3466
{
    public const uint WINDOW_STATE_DEFAULT = 0;
    public const uint WINDOW_STATE_ACTIVE = 1;              // 1 << 0
    public const uint WINDOW_STATE_FOCUSED = 2;             // 1 << 1
    public const uint WINDOW_STATE_HOVERING = 4;            // 1 << 2
    public const uint WINDOW_STATE_SELECTED = 8;            // 1 << 3
    public const uint WINDOW_STATE_PRESSED = 16;            // 1 << 4
    public const uint WINDOW_STATE_DISABLED = 32;           // 1 << 5
    public const uint WINDOW_STATE_LOCKED = 64;             // 1 << 6
    public const uint WINDOW_STATE_DESTROYING = 1073741824; // 1 << 30
}
