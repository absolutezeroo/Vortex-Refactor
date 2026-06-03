// @see core/window/dynamicstyle/class_3622.as

namespace Vortex.Core.Window.Dynamicstyle;

/// <summary>
/// Static style registry with lazy initialization.
/// Provides predefined dynamic styles: "lifted_hover" and "brightness_and_shadow_under".
/// </summary>
/// @see core/window/dynamicstyle/class_3622.as
public static class Class3622
{
    private static Dictionary<string, DynamicStyle>? _styles;

    /// @see class_3622.as::getStyle
    public static DynamicStyle GetStyle(string name)
    {
        if (_styles == null)
        {
            FillStyleTable();
        }

        if (_styles!.TryGetValue(name, out DynamicStyle? style))
        {
            return style;
        }

        return new DynamicStyle();
    }

    /// @see class_3622.as::fillStyleTable
    private static void FillStyleTable()
    {
        _styles = new Dictionary<string, DynamicStyle>();

        // "lifted_hover" style
        DynamicStyle liftedHover = new("lifted_hover")
        {
            DefaultStyles = new Dictionary<string, object?>(),
            PressedStyles = new Dictionary<string, object?>
            {
                ["offsetX"] = 1.0,
                ["colorTransform"] = new double[]
                {
                    1,
                    0.7,
                    0.7,
                    0.7,
                    0,
                    0,
                    0,
                    0,
                },
            },
            HoverStyles = new Dictionary<string, object?>
            {
                ["offsetY"] = -1.0,
                ["offsetX"] = -1.0,
            },
        };

        // Child style #icon for lifted_hover
        DynamicStyle liftedIcon = new()
        {
            DefaultStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 1207959552u,
                ["etchingPoint"] = new double[]
                {
                    1,
                    1,
                },
            },
            HoverStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 2147483648u,
                ["etchingPoint"] = new double[]
                {
                    2,
                    2,
                },
            },
            PressedStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 1207959552u,
                ["etchingPoint"] = new double[]
                {
                    -1,
                    -1,
                },
            },
        };
        liftedHover.ChildStyles["#icon"] = liftedIcon;

        // "brightness_and_shadow_under" style
        DynamicStyle brightnessUnder = new("brightness_and_shadow_under")
        {
            DefaultStyles = new Dictionary<string, object?>(),
        };

        // Child style #icon for brightness_and_shadow_under
        DynamicStyle brightnessIcon = new()
        {
            DefaultStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 1207959552u,
                ["etchingPoint"] = new double[]
                {
                    0,
                    1,
                },
            },
            PressedStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 2147483648u,
                ["etchingPoint"] = new double[]
                {
                    0,
                    -1,
                },
                ["offsetY"] = -1.0,
                ["colorTransform"] = new double[]
                {
                    0.7,
                    0.7,
                    0.7,
                    1,
                    0,
                    0,
                    0,
                    0,
                },
            },
            HoverStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 1207959552u,
                ["etchingPoint"] = new double[]
                {
                    0,
                    1,
                },
                ["colorTransform"] = new double[]
                {
                    1,
                    1,
                    1,
                    1,
                    77,
                    77,
                    77,
                    0,
                },
            },
        };
        brightnessUnder.ChildStyles["#icon"] = brightnessIcon;

        // Child style #bg for brightness_and_shadow_under
        DynamicStyle brightnessBg = new()
        {
            DefaultStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 1207959552u,
                ["etchingPoint"] = new double[]
                {
                    0,
                    1,
                },
            },
            PressedStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 2147483648u,
                ["etchingPoint"] = new double[]
                {
                    0,
                    0,
                },
                ["colorTransform"] = new double[]
                {
                    0.9,
                    0.9,
                    0.9,
                    1,
                    0,
                    0,
                    0,
                    0,
                },
            },
            HoverStyles = new Dictionary<string, object?>
            {
                ["etchingColor"] = 1207959552u,
                ["etchingPoint"] = new double[]
                {
                    0,
                    1,
                },
                ["colorTransform"] = new double[]
                {
                    1,
                    1,
                    1,
                    1,
                    77,
                    77,
                    77,
                    0,
                },
            },
            DisabledStyles = new Dictionary<string, object?>
            {
                ["colorTransform"] = new double[]
                {
                    0.5,
                    0.5,
                    0.5,
                    0.7,
                    0,
                    0,
                    0,
                    0,
                },
            },
        };
        brightnessUnder.ChildStyles["#bg"] = brightnessBg;

        _styles["lifted_hover"] = liftedHover;
        _styles["brightness_and_shadow_under"] = brightnessUnder;
    }
}
