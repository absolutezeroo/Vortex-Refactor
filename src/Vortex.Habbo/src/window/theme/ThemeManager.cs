// @see WIN63-202407091256-704579380-Source-main/habbo/window/theme/ThemeManager.as

using System;
using System.Linq;

using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Theme;

namespace Vortex.Habbo.Window.Theme;

/// @see WIN63-202407091256-704579380-Source-main/habbo/window/theme/ThemeManager.as
public class ThemeManager : IThemeManager
{
    /// @see ThemeManager.as::THEMES
    private static readonly string[] THEMES =
        [Theme.NONE, Theme.VOLTER, Theme.UBUNTU, Theme.ILLUMINA_LIGHT, Theme.ILLUMINA_DARK, Theme.ICON, Theme.LEGACY_BORDER];

    private readonly Dictionary<string, Theme> _themes = new(StringComparer.Ordinal);

    private readonly ISkinContainer? _skinContainer;

    /// @see ThemeManager.as::ThemeManager
    public ThemeManager(ISkinContainer? param1 = null)
    {
        _skinContainer = param1;

        PropertyMap baseDefaults = new();

        // @see ThemeManager.as::ThemeManager — ~95 property definitions
        baseDefaults.AddBoolean(Class3594.ALWAYS_SHOW_SELECTION, false);
        baseDefaults.AddEnumeration(Class3594.ANTIALIAS_TYPE, "advanced", Class3594.ANTIALIAS_TYPE_RANGE);
        baseDefaults.AddString(Class3594.ASSET_URI, null);
        baseDefaults.AddBoolean(Class3594.AUTO_ARRANGE_ITEMS, true);
        baseDefaults.AddEnumeration(Class3594.AUTO_SIZE, "none", ["none", "left", "center", "right"]);
        baseDefaults.AddString(Class3594.BITMAP_ASSET_NAME, null);
        baseDefaults.AddBoolean(Class3594.BORDER, false);
        baseDefaults.AddHex(Class3594.BORDER_COLOR, 0);
        baseDefaults.AddBoolean(Class3594.CONDENSE_WHITE, false);
        baseDefaults.AddBoolean(Class3594.CONTAINER_RESIZE_TO_COLUMNS, false);
        baseDefaults.AddEnumeration(Class3594.DIRECTION, "down", ["up", "down", "left", "right"]);
        baseDefaults.AddBoolean(Class3594.DISPLAY_AS_PASSWORD, false);
        baseDefaults.AddBoolean(Class3594.DISPLAY_RAW, false);
        baseDefaults.AddBoolean(Class3594.EDITABLE, true);
        baseDefaults.AddHex(Class3594.ETCHING_COLOR, 0);
        baseDefaults.AddBoolean(Class3594.FIT_SIZE_TO_CONTENTS, false);
        baseDefaults.AddBoolean(Class3594.FOCUS_CAPTURER, false);
        baseDefaults.AddBoolean(Class3594.GREYSCALE, false);
        baseDefaults.AddEnumeration(Class3594.GRID_FIT_TYPE, "pixel", ["pixel", "none", "subpixel"]);
        baseDefaults.AddBoolean(Class3594.HANDLE_BITMAP_DISPOSING, true);
        baseDefaults.AddString(Class3594.HELP_PAGE, "");
        baseDefaults.AddEnumeration(Class3594.HTML_LINK_TARGET, "default", Class3594.HTML_LINK_TARGET_RANGE);
        baseDefaults.AddInt(Class3594.ITEM_SPACING, 0);
        baseDefaults.AddInt(Class3594.MARGIN_LEFT, 0);
        baseDefaults.AddInt(Class3594.MARGIN_TOP, 0);
        baseDefaults.AddInt(Class3594.MARGIN_RIGHT, 0);
        baseDefaults.AddInt(Class3594.MARGIN_BOTTOM, 0);
        baseDefaults.AddInt(Class3594.MAX_CHARS, 0);
        baseDefaults.AddInt(Class3594.MAX_LINES, 0);
        baseDefaults.AddArray(Class3594.MENU_ITEM_ARRAY, []);
        baseDefaults.AddBoolean(Class3594.MOUSE_WHEEL_ENABLED, true);
        baseDefaults.AddBoolean(Class3594.MULTILINE, false);
        baseDefaults.AddEnumeration(Class3594.PIVOT_POINT, Class3594.PIVOT_POINT_RANGE[0], Class3594.PIVOT_POINT_RANGE);
        baseDefaults.AddInt(Class3594.POINTER_OFFSET, 0);
        baseDefaults.AddBoolean(Class3594.RESIZE_ON_ITEM_UPDATE, false);
        baseDefaults.AddString(Class3594.RESTRICT, null);
        baseDefaults.AddBoolean(Class3594.SCALE_TO_FIT_ITEMS, false);
        baseDefaults.AddString(Class3594.SCROLLABLE, "");
        baseDefaults.AddNumber(Class3594.SCROLL_STEP_HORIZONTAL, -1);
        baseDefaults.AddNumber(Class3594.SCROLL_STEP_VERTICAL, -1);
        baseDefaults.AddBoolean(Class3594.SELECTABLE, true);
        baseDefaults.AddBoolean(Class3594.STRETCHED_X, true);
        baseDefaults.AddBoolean(Class3594.STRETCHED_Y, true);
        baseDefaults.AddHex(Class3594.TEXT_COLOR, 0);
        baseDefaults.AddEnumeration(
            Class3594.TEXT_STYLE, "regular",
            Core.Window.Utils.TextStyleManager.EnumerateStyleNames().ToArray()
        );
        baseDefaults.AddString(Class3594.TOOL_TIP_CAPTION, "");
        baseDefaults.AddUint(Class3594.TOOL_TIP_DELAY, 500);
        baseDefaults.AddBoolean(Class3594.TOOL_TIP_IS_DYNAMIC, false);
        baseDefaults.AddBoolean(Class3594.INTERACTIVE_CURSOR_DISABLED, false);
        baseDefaults.AddBoolean(Class3594.VERTICAL, false);
        baseDefaults.AddEnumeration(
            Class3594.WIDGET_TYPE, "",
            Widgets.Class3474.WIDGET_TYPES
        );
        baseDefaults.AddBoolean(Class3594.WORD_WRAP, false);
        baseDefaults.AddNumber(Class3594.ZOOM_X, 1);
        baseDefaults.AddNumber(Class3594.ZOOM_Y, 1);
        baseDefaults.AddBoolean(Class3594.OPEN_UPWARD, false);
        baseDefaults.AddBoolean(Class3594.KEEP_OPEN_ON_DEACTIVATE, false);
        baseDefaults.AddInt(Class3594.PADDING_HORIZONTAL, 6);
        baseDefaults.AddInt(Class3594.PADDING_VERTICAL, 6);
        baseDefaults.AddString(Class3594.OVERFLOW_REPLACE, "");
        baseDefaults.AddBoolean(Class3594.WRAP_X, false);
        baseDefaults.AddBoolean(Class3594.WRAP_Y, false);
        baseDefaults.AddNumber(Class3594.ROTATION, 0);
        baseDefaults.AddEnumeration(
            Widgets.IlluminaBorderWidget.BORDER_STYLE_KEY,
            Widgets.IlluminaBorderWidget.BORDER_STYLE_ILLUMINA_LIGHT,
            Widgets.IlluminaBorderWidget.BORDER_STYLES
        );

        // @see ThemeManager.as — create 7 Theme instances
        _themes[Theme.NONE] = new Theme(Theme.NONE, false, 0, uint.MaxValue, baseDefaults);

        // @see ThemeManager.as — Icon style count from SkinContainer query
        uint iconStyleCount = 0;
        if (param1 != null)
        {
            while (param1.SkinRendererExists(1, iconStyleCount))
            {
                iconStyleCount++;
            }
        }

        _themes[Theme.ICON] = new Theme(Theme.ICON, false, 0, iconStyleCount, baseDefaults);

        // @see ThemeManager.as — Legacy border style count from SkinContainer query
        uint legacyBorderStyleCount = 0;
        if (param1 != null)
        {
            while (param1.SkinRendererExists(30, legacyBorderStyleCount) && legacyBorderStyleCount < 100)
            {
                legacyBorderStyleCount++;
            }
        }

        _themes[Theme.LEGACY_BORDER] = new Theme(Theme.LEGACY_BORDER, false, 0, legacyBorderStyleCount, baseDefaults);

        _themes[Theme.VOLTER] = new Theme(Theme.VOLTER, true, 0, 3, baseDefaults.Clone());

        PropertyMap ubuntuDefaults = baseDefaults.Clone();
        ubuntuDefaults.AddEnumeration(Class3594.ANTIALIAS_TYPE, "advanced", Class3594.ANTIALIAS_TYPE_RANGE);
        ubuntuDefaults.AddEnumeration(Class3594.TEXT_STYLE, "u_regular", ["regular", "u_regular"]);
        _themes[Theme.UBUNTU] = new Theme(Theme.UBUNTU, true, 3, 5, ubuntuDefaults);

        // @see ThemeManager.as — AS3 applies etching_color to baseDefaults (shared ref), not the clone.
        PropertyMap illuminaLightDefaults = baseDefaults.Clone();
        illuminaLightDefaults.AddEnumeration(Class3594.ANTIALIAS_TYPE, "advanced", Class3594.ANTIALIAS_TYPE_RANGE);
        baseDefaults.AddHex(Class3594.ETCHING_COLOR, 0xB2F19F7F);
        illuminaLightDefaults.AddEnumeration(Class3594.TEXT_STYLE, "il_regular", ["regular", "il_regular"]);
        _themes[Theme.ILLUMINA_LIGHT] = new Theme(Theme.ILLUMINA_LIGHT, true, 100, 100, illuminaLightDefaults);

        // @see ThemeManager.as — AS3 applies border_style to baseDefaults (shared ref), not the clone.
        PropertyMap illuminaDarkDefaults = illuminaLightDefaults.Clone();
        baseDefaults.AddEnumeration("illumina_border:border_style", "illumina_dark", ["illumina_light", "illumina_dark"]);
        _themes[Theme.ILLUMINA_DARK] = new Theme(Theme.ILLUMINA_DARK, true, 200, 100, illuminaDarkDefaults);
    }

    /// @see ThemeManager.as::getStyle
    public uint GetStyle(string param1, uint param2, string param3)
    {
        if (param1 == Theme.NONE)
        {
            return uint.TryParse(param3, out uint parsed) ? parsed : 0;
        }

        if (!_themes.TryGetValue(param1, out Theme? theme))
        {
            return 0;
        }

        for (uint i = 0;
             i < theme.styleCount;
             i++)
        {
            uint style = theme.baseStyle + i;

            if (param3 == _skinContainer?.GetIntentByTypeAndStyle(param2, style))
            {
                return style;
            }
        }

        return theme.baseStyle;
    }

    /// @see ThemeManager.as::getThemeAndIntent
    public (string theme, string? intent) GetThemeAndIntent(uint param1, uint param2)
    {
        string? intent = _skinContainer?.GetIntentByTypeAndStyle(param1, param2);

        switch (param1)
        {
            case 1:
                return (Theme.ICON, intent);
            case 30 when param2 < 100:
                return (Theme.LEGACY_BORDER, intent);
        }

        foreach (Theme theme in _themes.Values.Where(theme => theme.isReal && theme.CoversStyle(param2)))
        {
            return (theme.name, intent);
        }

        return (Theme.NONE, intent);
    }

    /// @see ThemeManager.as::getIntents
    public string[] GetIntents(uint param1, string param2, uint param3)
    {
        if (param2 == Theme.NONE || !_themes.TryGetValue(param2, out Theme? theme))
        {
            return [param3.ToString()];
        }

        List<string> intents = new();

        for (uint i = 0;
             i < theme.styleCount;
             i++)
        {
            string? intent = _skinContainer?.GetIntentByTypeAndStyle(param1, theme.baseStyle + i);

            if (intent != null)
            {
                intents.Add(intent);
            }
        }

        if (intents.Count > 0)
        {
            return intents.ToArray();
        }

        return [param3.ToString()];
    }

    /// @see ThemeManager.as::getPropertyDefaults
    public IPropertyMap? GetPropertyDefaults(uint param1)
    {
        foreach (Theme theme in _themes.Values.Where(theme => theme.isReal && theme.CoversStyle(param1)))
        {
            return theme.propertyDefaults;
        }

        return new PropertyMap();
    }

    /// @see ThemeManager.as::getThemes
    public string[] GetThemes()
    {
        return THEMES;
    }
}
