// @see core/window/utils/TextFieldCache.as

using System.IO;

using Godot;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Godot adaptation of AS3 TextFieldCache.
/// Instead of caching Flash TextFields, caches resolved Godot Font + metadata
/// needed for text measurement and rendering.
/// </summary>
/// @see core/window/utils/TextFieldCache.as
public static class TextFieldCache
{
    private static readonly Dictionary<string, CachedFontEntry> _cache = new();
    private static bool _listening;

    /// <summary>
    /// Resolved font entry with Godot Font, font size, and style metadata.
    /// </summary>
    public record CachedFontEntry(Font Font, int FontSize, bool Bold, bool Italic, TextStyle Style);

    /// @see TextFieldCache.as::getTextFieldByStyle
    public static CachedFontEntry? GetByStyle(TextStyle? style)
    {
        style ??= TextStyleManager.GetStyle("regular");

        if (style?.Name == null)
        {
            return null;
        }

        EnsureListening();

        if (_cache.TryGetValue(style.Name, out CachedFontEntry? cached))
        {
            return cached;
        }

        Font? font = ResolveFont(style);

        if (font == null)
        {
            return null;
        }

        int fontSize = style.FontSize ?? 12;
        bool bold = style.FontWeight == "bold";
        bool italic = style.FontStyle == "italic";

        CachedFontEntry entry = new(font, fontSize, bold, italic, style);
        _cache[style.Name] = entry;

        return entry;
    }

    /// @see TextFieldCache.as::getTextFieldByStyleName
    public static CachedFontEntry? GetByStyleName(string? styleName)
    {
        if (styleName == null)
        {
            return null;
        }

        TextStyle? style = TextStyleManager.GetStyle(styleName);

        return style == null ? null : GetByStyle(style);
    }

    /// @see TextFieldCache.as::onTextStyleChanged
    private static void OnTextStyleChanged()
    {
        _cache.Clear();
    }

    private static void EnsureListening()
    {
        if (_listening)
        {
            return;
        }

        TextStyleManager.StyleChanged += OnTextStyleChanged;
        _listening = true;
    }

    /// <summary>
    /// Resolves a Godot Font file from a Class3613 style.
    /// Maps AS3 font-family + weight/style to files in assets/fonts/.
    /// </summary>
    private static Font? ResolveFont(TextStyle style)
    {
        string family = style.FontFamily ?? "Ubuntu";
        bool bold = style.FontWeight == "bold";
        bool italic = style.FontStyle == "italic";

        string fileName = family switch
        {
            "Ubuntu" when bold && italic => "Ubuntu-ib.ttf",
            "Ubuntu" when bold => "Ubuntu-b.ttf",
            "Ubuntu" when italic => "Ubuntu-i.ttf",
            "Ubuntu" => "Ubuntu.ttf",
            "UbuntuCondensed" => "Ubuntu-C.ttf",
            "Volter Bold" => "Volter Bold.ttf",
            "Volter" => "Volter.ttf",
            // Fallback: try direct family name
            _ => $"{family}.ttf",
        };

        string resPath = $"res://assets/fonts/{fileName}";
        string? absPath = ProjectSettings.GlobalizePath(resPath);

        if (!File.Exists(absPath))
        {
            GD.PrintErr($"[TextFieldCache] Font not found: {absPath}");

            return null;
        }

        FontFile font = new();
        Error err = font.LoadDynamicFont(absPath);

        if (err == Error.Ok)
        {
            return font;
        }

        GD.PrintErr($"[TextFieldCache] Failed to load font: {absPath} (error: {err})");

        return null;

    }
}
