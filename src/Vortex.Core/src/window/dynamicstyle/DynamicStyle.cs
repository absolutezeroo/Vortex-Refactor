// @see core/window/dynamicstyle/DynamicStyle.as

using System;
using System.Linq;

using Godot;

namespace Vortex.Core.Window.Dynamicstyle;

/// <summary>
/// Dynamic style definition with per-state style dictionaries (hover, pressed, disabled, default)
/// and child styles keyed by "#tag" for hierarchical window styling.
/// </summary>
/// @see core/window/dynamicstyle/DynamicStyle.as
public class DynamicStyle
{
    public const string STYLE_LIFTED_HOVER = "lifted_hover";
    public const string BRIGHTNESS_AND_SHADOW_UNDER = "brightness_and_shadow_under";

    public string Name;

    /// @see DynamicStyle.as::var_3167 — hover styles
    public Dictionary<string, object?> HoverStyles;

    /// @see DynamicStyle.as::defaultStyles
    public Dictionary<string, object?> DefaultStyles;

    /// @see DynamicStyle.as::pressedSyles (sic — matches AS3 typo)
    public Dictionary<string, object?> PressedStyles;

    /// @see DynamicStyle.as::var_4228 — disabled styles
    public Dictionary<string, object?> DisabledStyles;

    /// @see DynamicStyle.as::var_3172 — child styles by "#tag" key
    public Dictionary<string, DynamicStyle> ChildStyles;

    /// @see DynamicStyle.as::DynamicStyle
    public DynamicStyle(string name = "")
    {
        Name = name;
        HoverStyles = new Dictionary<string, object?>();
        DefaultStyles = new Dictionary<string, object?>();
        PressedStyles = new Dictionary<string, object?>();
        DisabledStyles = new Dictionary<string, object?>
        {
            ["colorTransform"] = new double[]
            {
                1,
                1,
                1,
                0.4,
                0,
                0,
                0,
                0,
            },
        };
        ChildStyles = new Dictionary<string, DynamicStyle>();
    }

    /// @see DynamicStyle.as::getStyleByWindowState
    public Dictionary<string, object?> GetStyleByWindowState(uint stateFlags)
    {
        return stateFlags switch
        {
            16 => PressedStyles,  // PRESSED
            4 => HoverStyles,     // HOVERING
            0 => DefaultStyles,   // DEFAULT
            32 => DisabledStyles, // DISABLED
            _ => new Dictionary<string, object?>(),
        };
    }

    // Scaffold-compatible overload
    public virtual object? GetStyleByWindowState(params object?[] args)
    {
        if (args is
            [
                uint s, ..,
            ])
        {
            return GetStyleByWindowState(s);
        }

        return new Dictionary<string, object?>();
    }

    /// @see DynamicStyle.as::getChildDynamicStyleByKey
    private DynamicStyle GetChildDynamicStyleByKey(string key)
    {
        if (ChildStyles.TryGetValue(key, out DynamicStyle? style))
        {
            return style;
        }

        return new DynamicStyle();
    }

    // Scaffold-compatible overload
    public virtual object? GetChildDynamicStyleByKey(params object?[] args)
    {
        if (args is
            [
                string key, ..,
            ])
        {
            return GetChildDynamicStyleByKey(key);
        }

        return new DynamicStyle();
    }

    /// @see DynamicStyle.as::getChildStyle
    public DynamicStyle? GetChildStyle(WindowController controller)
    {
        return (from tag in controller.tags where tag.Length > 0 && tag[0] == '#' select GetChildDynamicStyleByKey(tag))
            .FirstOrDefault();
    }

    // Scaffold-compatible overload
    public virtual object? GetChildStyle(params object?[] args)
    {
        if (args is
            [
                WindowController wc, ..,
            ])
        {
            return GetChildStyle(wc);
        }

        return null;
    }

    /// <summary>
    /// Compute a color uint from the colorTransform array for the given state.
    /// colorTransform format: [rMult, gMult, bMult, aMult, rOff, gOff, bOff, aOff]
    /// </summary>
    /// @see DynamicStyle.as::getColorValue
    public uint? GetColorValue(uint stateFlags)
    {
        Dictionary<string, object?> style = GetStyleByWindowState(stateFlags);

        if (!style.TryGetValue("colorTransform", out object? ctObj) || ctObj is not double[] ct || ct.Length < 8)
        {
            return null;
        }

        uint result = 0;

        for (int i = 0;
             i < 3;
             i++)
        {
            int channel = (int)((ct[i] * 255) + ct[i + 4]);
            channel = Math.Min(255, Math.Max(0, channel));
            result = (result << 8) | (uint)channel;
        }

        return result;
    }

    // Scaffold-compatible overload
    public virtual object? GetColorValue(params object?[] args)
    {
        if (args is
            [
                uint s, ..,
            ])
        {
            return GetColorValue(s);
        }

        return null;
    }

    /// <summary>
    /// Build a Godot Color from the colorTransform array for the given state.
    /// Godot adaptation: returns Godot Color instead of Flash ColorTransform.
    /// </summary>
    /// @see DynamicStyle.as::getColorTransform
    public Color GetColorTransform(uint stateFlags)
    {
        Dictionary<string, object?> style = GetStyleByWindowState(stateFlags);

        if (!style.TryGetValue("colorTransform", out object? ctObj) || ctObj is not double[] ct || ct.Length < 8)
        {
            return Colors.White;
        }

        double[] tint = [255, 255, 255];

        if (style.TryGetValue("tint", out object? tintObj) && tintObj is double[] { Length: >= 3 } t)
        {
            tint = t;
        }

        float r = (float)(ct[0] * tint[0] / 255.0);
        float g = (float)(ct[1] * tint[1] / 255.0);
        float b = (float)(ct[2] * tint[2] / 255.0);
        float a = (float)ct[3];

        return new Color(
            Mathf.Clamp(r + (float)(ct[4] / 255.0), 0, 1),
            Mathf.Clamp(g + (float)(ct[5] / 255.0), 0, 1),
            Mathf.Clamp(b + (float)(ct[6] / 255.0), 0, 1),
            Mathf.Clamp(a + (float)(ct[7] / 255.0), 0, 1)
        );
    }

    // Scaffold-compatible overload
    public virtual object? GetColorTransform(params object?[] args)
    {
        if (args is
            [
                uint s, ..,
            ])
        {
            return GetColorTransform(s);
        }

        return Colors.White;
    }
}
