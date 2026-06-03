// @see core/window/enum/PivotPoint.as

using System;

namespace Vortex.Core.Window.Enum;

/// @see core/window/enum/PivotPoint.as
public static class PivotPoint
{
    /// @see PivotPoint.as::TOP_LEFT
    public const uint TOP_LEFT = 0;

    /// @see PivotPoint.as::TOP_CENTER
    public const uint TOP_CENTER = 1;

    /// @see PivotPoint.as::TOP_RIGHT
    public const uint TOP_RIGHT = 2;

    /// @see PivotPoint.as::CENTER_LEFT
    public const uint CENTER_LEFT = 3;

    /// @see PivotPoint.as::CENTER
    public const uint CENTER = 4;

    /// @see PivotPoint.as::CENTER_RIGHT
    public const uint CENTER_RIGHT = 5;

    /// @see PivotPoint.as::BOTTOM_LEFT
    public const uint BOTTOM_LEFT = 6;

    /// @see PivotPoint.as::BOTTOM_CENTER
    public const uint BOTTOM_CENTER = 7;

    /// @see PivotPoint.as::BOTTOM_RIGHT
    public const uint BOTTOM_RIGHT = 8;

    /// @see PivotPoint.as::PIVOT_NAMES
    public static readonly string[] PIVOT_NAMES =
    [
        "top left", "top center", "top right",
        "center left", "center", "center right",
        "bottom left", "bottom center", "bottom right",
    ];

    /// @see PivotPoint.as::pivotFromName
    public static uint PivotFromName(string param1)
    {
        int idx = Array.IndexOf(PIVOT_NAMES, param1);

        return idx >= 0 ? (uint)idx : 0;
    }
}
