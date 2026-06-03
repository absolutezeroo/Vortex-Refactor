// Godot adaptation for Flash BitmapData.draw(textField).
// Uses Font.GetStringSize() for measurement and a SubViewport + Label for rasterization.

using Godot;

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Rasterizes text strings into Image buffers using a reusable SubViewport + Label.
/// This is the Godot equivalent of AS3's BitmapData.draw(textField).
/// </summary>
public static class TextImageRenderer
{
    private static SubViewport? _viewport;
    private static Label? _label;
    private static bool _initialized;

    private static readonly Dictionary<(string text, string styleName, uint color, int width), Image> _cache = new();

    /// <summary>
    /// Creates the singleton SubViewport + Label for text rasterization.
    /// Must be called once after a Node is available in the scene tree.
    /// </summary>
    public static void Initialize(Node parent)
    {
        if (_initialized)
        {
            return;
        }

        _viewport = new SubViewport
        {
            TransparentBg = true,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Disabled,
            GuiDisableInput = true,
            HandleInputLocally = false,
            Size = new Vector2I(256, 64),
            Name = "TextImageRendererViewport",
        };

        _label = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.Off,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            ClipText = false,
            Name = "TextRasterLabel",
        };

        _viewport.AddChild(_label);
        parent.AddChild(_viewport);

        _initialized = true;
    }

    /// <summary>
    /// Renders text into an Image. Returns cached result if available.
    /// </summary>
    public static Image? RenderText(string text, Font font, int fontSize, Color color, int maxWidth, string cacheKey)
    {
        if (!_initialized || _viewport == null || _label == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        uint colorUint = ((uint)color.R8 << 16) | ((uint)color.G8 << 8) | (uint)color.B8;
        (string text, string cacheKey, uint colorUint, int maxWidth) key = (text, cacheKey, colorUint, maxWidth);

        if (_cache.TryGetValue(key, out Image? cached))
        {
            return cached;
        }

        // Configure label
        _label.Text = text;
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", fontSize);
        _label.AddThemeColorOverride("font_color", color);
        _label.Position = Vector2.Zero;

        // Measure text size
        Vector2 textSize = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
        int w = Mathf.Max((int)Mathf.Ceil(textSize.X) + 2, 1);
        int h = Mathf.Max((int)Mathf.Ceil(textSize.Y) + 2, 1);

        _label.Size = new Vector2(w, h);
        _viewport.Size = new Vector2I(w, h);

        // Godot adaptation: trigger SubViewport render and force a full draw pass.
        // AS3's BitmapData.draw(textField) is synchronous; Godot's SubViewport renders
        // asynchronously. ForceSync() only syncs the command queue, not the viewport.
        // ForceDraw(false) forces a complete render pass including all SubViewports.
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        RenderingServer.ForceDraw(false);

        ViewportTexture? texture = _viewport.GetTexture();
        Image? image = texture?.GetImage();

        if (image == null)
        {
            return null;
        }

        // Store in cache
        _cache[key] = image;

        return image;
    }

    /// <summary>
    /// Renders text with a specific color override (for etching).
    /// Does not cache — etching images are rendered on demand.
    /// </summary>
    public static Image? RenderTextUncached(string text, Font font, int fontSize, Color color)
    {
        if (!_initialized || _viewport == null || _label == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        _label.Text = text;
        _label.AddThemeFontOverride("font", font);
        _label.AddThemeFontSizeOverride("font_size", fontSize);
        _label.AddThemeColorOverride("font_color", color);
        _label.Position = Vector2.Zero;

        Vector2 textSize = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
        int w = Mathf.Max((int)Mathf.Ceil(textSize.X) + 2, 1);
        int h = Mathf.Max((int)Mathf.Ceil(textSize.Y) + 2, 1);

        _label.Size = new Vector2(w, h);
        _viewport.Size = new Vector2I(w, h);

        // Godot adaptation: same ForceDraw approach as RenderText.
        _viewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Once;
        RenderingServer.ForceDraw(false);

        ViewportTexture? texture = _viewport.GetTexture();

        return texture?.GetImage();
    }

    /// <summary>
    /// Invalidates cached entries for a given style name.
    /// </summary>
    public static void InvalidateCache(string? styleName)
    {
        if (styleName == null)
        {
            _cache.Clear();
            return;
        }

        List<(string, string, uint, int)> toRemove = new();
        foreach ((string text, string styleName, uint color, int width) key in _cache.Keys)
        {
            if (key.styleName == styleName)
            {
                toRemove.Add(key);
            }
        }
        foreach ((string, string, uint, int) key in toRemove)
        {
            _cache.Remove(key);
        }
    }

    /// <summary>
    /// Clears the entire text image cache.
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
    }
}
