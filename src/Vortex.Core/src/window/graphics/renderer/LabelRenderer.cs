// @see core/window/graphics/renderer/LabelRenderer.as

using Godot;

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/LabelRenderer.as
public class LabelRenderer : SkinRenderer
{
    private TextStyle? _cachedStyle;
    private TextFieldCache.CachedFontEntry? _cachedFontEntry;

    /// @see LabelRenderer.as::LabelRenderer
    public LabelRenderer(string name) : base(name) { }

    /// @see LabelRenderer.as::draw
    public override void Draw(IWindow window, Image buffer, Rect2 region, uint state, bool flag)
    {
        if (buffer == null)
        {
            return;
        }

        if (window is not TextLabelController label)
        {
            return;
        }

        TextStyle? style = label.TextStyle;

        if (style == null)
        {
            return;
        }

        // Cache font entry when style changes
        if (style != _cachedStyle)
        {
            _cachedFontEntry = TextFieldCache.GetByStyle(style);
            _cachedStyle = style;
        }

        if (_cachedFontEntry == null)
        {
            return;
        }

        string text = label.text;
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        int offsetX = label.DrawOffsetX;
        int offsetY = label.DrawOffsetY;

        // Determine text color
        uint styleColor = style.Color ?? 0;
        uint drawColor = label.HasTextColor ? label.TextColorValue : styleColor;
        Color godotColor = new(
            ((drawColor >> 16) & 0xFF) / 255f,
            ((drawColor >> 8) & 0xFF) / 255f,
            (drawColor & 0xFF) / 255f,
            1f
        );

        // Render main text image
        Image? textImage = TextImageRenderer.RenderText(
            text,
            _cachedFontEntry.Font,
            _cachedFontEntry.FontSize,
            godotColor,
            (int)region.Size.X,
            label.TextStyleName ?? "u_regular"
        );

        if (textImage == null)
        {
            return;
        }

        // Etching: render shadow/outline text if etchingColor has alpha > 0
        if (style.EtchingColor != null)
        {
            uint etchColor = style.EtchingColor.Value;

            // @see LabelRenderer.as — check alpha channel: (etchingColor & 0xFF000000) != 0
            if ((etchColor & 0xFF000000) != 0 && style.EtchingPosition != null)
            {
                if (ETCHING_POSITION.TryGetValue(style.EtchingPosition, out (int x, int y) etchOffset))
                {
                    // Etching color: use RGB from etchingColor, rendered as a ColorTransform
                    // @see LabelRenderer.as — const_418 is (0,0,0,1, R,G,B,0) meaning: replace RGB, keep alpha
                    byte etchR = (byte)((etchColor >> 16) & 0xFF);
                    byte etchG = (byte)((etchColor >> 8) & 0xFF);
                    byte etchB = (byte)(etchColor & 0xFF);
                    Color etchGodotColor = new(etchR / 255f, etchG / 255f, etchB / 255f, 1f);

                    Image? etchImage = TextImageRenderer.RenderTextUncached(
                        text,
                        _cachedFontEntry.Font,
                        _cachedFontEntry.FontSize,
                        etchGodotColor
                    );

                    if (etchImage != null)
                    {
                        BitmapSkinRenderer.BlitImage(
                            buffer, etchImage,
                            offsetX + etchOffset.x,
                            offsetY + etchOffset.y
                        );
                    }
                }
            }
        }

        // Blit main text
        BitmapSkinRenderer.BlitImage(buffer, textImage, offsetX, offsetY);
    }

    /// @see LabelRenderer.as::isStateDrawable
    public override bool IsStateDrawable(uint state)
    {
        return state == 0;
    }
}
