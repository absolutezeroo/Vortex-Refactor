// @see core/window/graphics/class_3655.as

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Invalidation flags for window rendering.
/// </summary>
/// @see core/window/graphics/class_3655.as
public static class Class3655
{
    public const uint UNKNOWN = 0;
    public const uint REDRAW = 1;
    public const uint RESIZE = 2;
    public const uint RELOCATE = 4;
    public const uint STATE = 8;
    public const uint BLEND = 16;
    public const uint CASCADE = 32;
}
