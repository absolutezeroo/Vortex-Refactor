// @see core/window/utils/class_3639.as

using System;
using System.Globalization;
using System.Linq;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3639.as
public static class TextStyleManager
{
    public const string REGULAR = "regular";
    public const string ITALIC = "italic";
    public const string BOLD = "bold";

    private static Dictionary<string, TextStyle> _styles = new();
    private static List<string> _styleNames = new();
    private static bool _initialized;

    /// @see class_3639.as::events — style changed event
    public static event Action? StyleChanged;

    /// @see class_3639.as::events — style added event
    public static event Action? StyleAdded;

    public static int StyleCount => _styles.Count;

    /// @see class_3639.as::init
    public static void Init()
    {
        _styles = new Dictionary<string, TextStyle>();
        _styleNames = new List<string>();

        TextStyle regular = new()
        {
            Name = "regular",
            Color = 0,
            FontSize = 9,
            FontFamily = "Courier",
            FontStyle = "normal",
            FontWeight = "normal",
        };
        _styles[regular.Name!] = regular;
        _styleNames.Add(regular.Name!);

        TextStyle italic = new()
        {
            Name = "italic",
            Color = 0,
            FontSize = 9,
            FontFamily = "Courier",
            FontStyle = "italic",
            FontWeight = "normal",
        };
        _styles[italic.Name!] = italic;
        _styleNames.Add(italic.Name!);

        TextStyle bold = new()
        {
            Name = "bold",
            Color = 0,
            FontSize = 9,
            FontFamily = "Courier",
            FontStyle = "normal",
            FontWeight = "bold",
        };
        _styles[bold.Name!] = bold;
        _styleNames.Add(bold.Name!);

        _initialized = true;
    }

    /// @see class_3639.as::getStyle
    public static TextStyle? GetStyle(string? name)
    {
        if (name == null)
        {
            return null;
        }

        _styles.TryGetValue(name, out TextStyle? style);

        return style;
    }

    /// @see class_3639.as::setStyle
    public static void SetStyle(string name, TextStyle style)
    {
        bool isNew = !_styles.ContainsKey(name);

        style.Name = name;

        _styles[name] = style;

        if (isNew)
        {
            _styleNames.Add(name);
            StyleAdded?.Invoke();
        }
        else
        {
            StyleChanged?.Invoke();
        }
    }

    /// @see class_3639.as::setStyles
    public static void SetStyles(IList<TextStyle> styles, bool clear = false)
    {
        if (clear)
        {
            TextStyle? savedRegular = _styles.GetValueOrDefault("regular");
            TextStyle? savedItalic = _styles.GetValueOrDefault("italic");
            TextStyle? savedBold = _styles.GetValueOrDefault("bold");

            _styles.Clear();

            if (savedRegular != null)
            {
                _styles["regular"] = savedRegular;
            }

            if (savedItalic != null)
            {
                _styles["italic"] = savedItalic;
            }

            if (savedBold != null)
            {
                _styles["bold"] = savedBold;
            }
        }

        int prevCount = _styles.Count;

        foreach (TextStyle style in styles)
        {
            if (style.Name == null)
            {
                continue;
            }

            _styles[style.Name] = style;

            if (!_styleNames.Contains(style.Name))
            {
                _styleNames.Add(style.Name);
            }
        }

        StyleChanged?.Invoke();

        if (_styles.Count != prevCount)
        {
            StyleAdded?.Invoke();
        }
    }

    /// @see class_3639.as::parseCSS
    public static List<TextStyle> ParseCss(string css)
    {
        List<string> styleNames = ParseStyleNamesFromCss(css);
        Dictionary<string, Dictionary<string, string>> parsedStyles = ParseCssProperties(css);
        List<TextStyle> result = new();

        foreach (string name in styleNames)
        {
            if (!parsedStyles.TryGetValue(name, out Dictionary<string, string>? props))
            {
                continue;
            }

            TextStyle style = new()
            {
                Name = name,
            };

            if (props.TryGetValue("color", out string? colorVal))
            {
                style.Color = ParseHexColor(colorVal);
            }

            if (props.TryGetValue("font-family", out string? ff))
            {
                style.FontFamily = ff;
            }

            if (props.TryGetValue("font-size", out string? fs) && int.TryParse(fs, out int fsInt))
            {
                style.FontSize = fsInt;
            }

            if (props.TryGetValue("font-style", out string? fst))
            {
                style.FontStyle = fst;
            }

            if (props.TryGetValue("font-weight", out string? fw))
            {
                style.FontWeight = fw;
            }

            if (props.TryGetValue("kerning", out string? k))
            {
                style.Kerning = k == "true";
            }

            if (props.TryGetValue("leading", out string? ld) && int.TryParse(ld, out int ldInt))
            {
                style.Leading = ldInt;
            }

            if (props.TryGetValue("letter-spacing", out string? ls) && int.TryParse(ls, out int lsInt))
            {
                style.LetterSpacing = lsInt;
            }

            if (props.TryGetValue("text-decoration", out string? td))
            {
                style.TextDecoration = td;
            }

            if (props.TryGetValue("text-indent", out string? ti) && int.TryParse(ti, out int tiInt))
            {
                style.TextIndent = tiInt;
            }

            if (props.TryGetValue("anti-alias-type", out string? aat))
            {
                style.AntiAliasType = aat;
            }

            if (props.TryGetValue("sharpness", out string? sh) && int.TryParse(sh, out int shInt))
            {
                style.Sharpness = shInt;
            }

            if (props.TryGetValue("thickness", out string? th) && int.TryParse(th, out int thInt))
            {
                style.Thickness = thInt;
            }

            if (props.TryGetValue("etching-color", out string? ec))
            {
                style.EtchingColor = ParseHexColor(ec);
            }

            if (props.TryGetValue("etching-position", out string? ep))
            {
                style.EtchingPosition = ep;
            }

            result.Add(style);
        }

        return result;
    }

    /// @see class_3639.as::enumerateStyles
    public static List<TextStyle> EnumerateStyles()
    {
        return _styles.Values.ToList();
    }

    /// @see class_3639.as::enumerateStyleNames
    public static List<string> EnumerateStyleNames()
    {
        return
        [
            .._styles.Keys,
        ];
    }

    /// @see class_3639.as::toString
    public static new string ToString()
    {
        return _styles.Values.Aggregate("", (current, style) => current + style.ToString() + "\n\n");
    }

    /// @see class_3639.as::parseStyleNamesFromCSS
    private static List<string> ParseStyleNamesFromCss(string css)
    {
        List<string> result = new();
        string cleaned = css.Replace("\t", "").Replace("\n", "").Replace("\r", "");

        // Validate matching braces
        int openCount = CountSubStrings(css, "{");
        int closeCount = CountSubStrings(css, "}");

        if (openCount != closeCount)
        {
            throw new Exception("Mismatching amount of \"{\" versus \"}\", please check the CSS!");
        }

        string[] blocks = cleaned.Split('}');

        foreach (string block in blocks)
        {
            string current = block;

            // Strip leading comments
            while (current.IndexOf("/*", StringComparison.Ordinal) == 0)
            {
                int endComment = current.IndexOf("*/", StringComparison.Ordinal);

                if (endComment < 0)
                {
                    break;
                }

                current = current[(endComment + 2)..];
            }

            int braceIdx = current.IndexOf('{');

            if (braceIdx < 0)
            {
                continue;
            }

            string name = current[..braceIdx].Replace(" ", "");

            if (name.Length > 0)
            {
                result.Add(name);
            }
        }

        return result;
    }

    private static Dictionary<string, Dictionary<string, string>> ParseCssProperties(string css)
    {
        Dictionary<string, Dictionary<string, string>> result = new();
        string cleaned = css.Replace("\t", "").Replace("\n", "").Replace("\r", "");
        string[] blocks = cleaned.Split('}');

        foreach (string block in blocks)
        {
            string current = block;

            // Strip leading comments
            while (current.IndexOf("/*", StringComparison.Ordinal) == 0)
            {
                int endComment = current.IndexOf("*/", StringComparison.Ordinal);
                if (endComment < 0)
                {
                    break;
                }
                current = current[(endComment + 2)..];
            }

            int braceIdx = current.IndexOf('{');

            if (braceIdx < 0)
            {
                continue;
            }

            string name = current[..braceIdx].Replace(" ", "");

            if (name.Length == 0)
            {
                continue;
            }

            string body = current[(braceIdx + 1)..];
            Dictionary<string, string> props = new();
            string[] lines = body.Split(';');

            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                int colonIdx = trimmed.IndexOf(':');

                if (colonIdx < 0)
                {
                    continue;
                }

                string key = trimmed[..colonIdx].Trim();
                string value = trimmed[(colonIdx + 1)..].Trim();

                if (key.Length > 0 && value.Length > 0)
                {
                    props[key] = value;
                }
            }

            result[name] = props;
        }

        return result;
    }

    /// @see class_3639.as — hex color parsing: #rrggbb (6-digit → opaque) or #aarrggbb (8-digit)
    private static uint? ParseHexColor(string value)
    {
        string hex = value.Replace("#", "").Replace("0x", "");

        if (hex.Length == 0)
        {
            return null;
        }

        if (!uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint parsed))
        {
            return null;
        }

        // 6-digit: treat as opaque (0xFF prefix)
        if (hex.Length <= 6)
        {
            return 0xFF000000 | parsed;
        }

        // 8-digit: already includes alpha
        return parsed;

    }

    /// @see class_3639.as::countSubStrings
    private static int CountSubStrings(string text, string sub)
    {
        int count = 0;
        int idx = 0;

        while ((idx = text.IndexOf(sub, idx, StringComparison.Ordinal)) != -1)
        {
            idx++;
            count++;
        }

        return count;
    }
}
