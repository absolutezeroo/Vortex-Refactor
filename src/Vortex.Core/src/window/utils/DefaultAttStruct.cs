// @see core/window/utils/DefaultAttStruct.as

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Default visual attributes for a window type+style combination.
/// </summary>
/// @see core/window/utils/DefaultAttStruct.as
public class DefaultAttStruct
{
    public static bool UseRectLimits = true;

    public uint Color { get; set; } = 0xFFFFFF;
    public bool Background { get; set; }
    public float Blend { get; set; } = 1.0f;
    public uint Threshold { get; set; } = 10;
    public int WidthMin { get; set; } = int.MinValue;
    public int WidthMax { get; set; } = int.MaxValue;
    public int HeightMin { get; set; } = int.MinValue;
    public int HeightMax { get; set; } = int.MaxValue;

    /// @see DefaultAttStruct.as::hasRectLimits
    public bool HasRectLimits()
    {
        return UseRectLimits &&
               (WidthMin > int.MinValue ||
                HeightMin > int.MinValue ||
                WidthMax < int.MaxValue ||
                HeightMax < int.MaxValue);
    }
}
