// @see habbo/window/utils/class_3822.as

namespace Vortex.Habbo.Window.Utils;

/// <summary>
/// Utility for determining room user count display color based on occupancy percentage.
/// </summary>
/// @see habbo/window/utils/class_3822.as
public static class Class3822
{
    /// @see class_3822.as::getUserCountColor
    /// <summary>
    /// Returns ARGB color based on room occupancy percentage.
    /// >= 92%: 0xFFC5332C (red), >= 50%: 0xFFECA01B (orange),
    /// > 0: 0xFF5EB5E2 (blue), 0: 0xFFA8A8A1 (grey).
    /// </summary>
    public static uint GetUserCountColor(int current, int max)
    {
        if (max <= 0)
        {
            return 0xFFA8A8A1;
        }

        int percent = 100 * current / max;

        if (percent >= 92)
        {
            return 0xFFC5332C;
        }
        if (percent >= 50)
        {
            return 0xFFECA01B;
        }
        if (current > 0)
        {
            return 0xFF5EB5E2;
        }
        return 0xFFA8A8A1;
    }
}
