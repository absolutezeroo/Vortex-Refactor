// @see core/window/graphics/renderer/TextSkinRenderer.as

using System;

using Godot;

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/TextSkinRenderer.as
public class TextSkinRenderer : SkinRenderer
{
    /// @see TextSkinRenderer.as::const_418 — etching color transform: zero RGB multipliers, offset replaces color
    private static readonly ColorTransform EtchingTransform = new(0, 0, 0, 1, 255, 255, 255, 0);

    /// @see TextSkinRenderer.as::TextSkinRenderer
    public TextSkinRenderer(string name) : base(name) { }

    /// @see TextSkinRenderer.as::parse
    public override void Parse(System.Xml.Linq.XElement? skinXml, System.Xml.Linq.XElement? xmlStates, Func<string, Image?>? assetResolver)
    {
        // @see TextSkinRenderer.as::parse — loads CSS content and registers styles
        // AS3: class_3639.setStyles(class_3639.parseCSS(param1.content.toString()))
        // In our port, the CSS asset content is passed as the skinXml's text value.
        if (skinXml == null)
        {
            return;
        }

        string cssContent = skinXml.Value;

        if (string.IsNullOrEmpty(cssContent))
        {
            return;
        }

        List<TextStyle> styles = TextStyleManager.ParseCss(cssContent);
        TextStyleManager.SetStyles(styles);
    }

    /// @see TextSkinRenderer.as::draw
    public override void Draw(IWindow window, Image? buffer, Rect2 region, uint state, bool flag)
    {
        if (buffer == null)
        {
            return;
        }

        if (window is not ITextFieldContainer fieldContainer)
        {
            return;
        }

        // Resolve font entry from the cached text field
        if (fieldContainer.TextField is not TextFieldCache.CachedFontEntry fontEntry)
        {
            return;
        }

        TextMargins? margins = fieldContainer.Margins as TextMargins;
        int marginLeft = margins?.LeftValue ?? 0;
        int marginTop = margins?.TopValue ?? 0;
        int marginRight = margins?.RightValue ?? 0;

        // Resolve text and autoSize from ITextWindow
        string text = "";
        string autoSize = "none";
        uint etchingColor = 0;
        string? etchingPosition = null;

        if (window is ITextWindow textWindow)
        {
            text = textWindow.Text ?? "";
            autoSize = textWindow.AutoSize ?? "none";
            etchingColor = textWindow.EtchingColor;
            etchingPosition = textWindow.EtchingPosition;
        }

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // Measure text for alignment
        Vector2 textSize = fontEntry.Font.GetStringSize(
            text, HorizontalAlignment.Left, -1, fontEntry.FontSize
        );
        int textW = (int)Math.Ceiling(textSize.X);

        // @see TextSkinRenderer.as — compute tx (horizontal offset)
        int tx = marginLeft;
        int ty = marginTop;

        if (autoSize == "right")
        {
            tx = (int)Math.Floor(window.width - textW - marginRight);
        }
        else if (autoSize == "center")
        {
            tx = (int)Math.Floor((window.width / 2f) - (textW / 2f));
        }

        // Determine text color from font entry style
        uint styleColor = fontEntry.Style.Color ?? 0;
        Color godotColor = new(
            ((styleColor >> 16) & 0xFF) / 255f,
            ((styleColor >> 8) & 0xFF) / 255f,
            (styleColor & 0xFF) / 255f,
            1f
        );

        // @see TextSkinRenderer.as — etching: draw shadow text if etchingColor has alpha
        if ((etchingColor & 0xFF000000) != 0 && etchingPosition != null)
        {
            if (ETCHING_POSITION.TryGetValue(etchingPosition, out (int x, int y) etchOffset))
            {
                byte etchR = (byte)((etchingColor >> 16) & 0xFF);
                byte etchG = (byte)((etchingColor >> 8) & 0xFF);
                byte etchB = (byte)(etchingColor & 0xFF);
                float etchA = ((etchingColor >> 24) & 0xFF) / 255f;
                Color etchGodotColor = new(etchR / 255f, etchG / 255f, etchB / 255f, etchA);

                Image? etchImage = TextImageRenderer.RenderTextUncached(
                    text, fontEntry.Font, fontEntry.FontSize, etchGodotColor
                );

                if (etchImage != null)
                {
                    BitmapSkinRenderer.BlitImage(
                        buffer, etchImage,
                        tx + etchOffset.x,
                        ty + etchOffset.y
                    );
                }
            }
        }

        // @see TextSkinRenderer.as — apply dynamicStyleColor if present
        Color renderColor = godotColor;

        if (window.dynamicStyleColor != null)
        {
            ColorTransform? ct = window.dynamicStyleColor;
            renderColor = new Color(
                Math.Clamp((godotColor.R * ct.RedMultiplier) + (ct.RedOffset / 255f), 0f, 1f),
                Math.Clamp((godotColor.G * ct.GreenMultiplier) + (ct.GreenOffset / 255f), 0f, 1f),
                Math.Clamp((godotColor.B * ct.BlueMultiplier) + (ct.BlueOffset / 255f), 0f, 1f),
                Math.Clamp((godotColor.A * ct.AlphaMultiplier) + (ct.AlphaOffset / 255f), 0f, 1f)
            );
        }

        // Render main text
        Image? textImage = TextImageRenderer.RenderText(
            text, fontEntry.Font, fontEntry.FontSize, renderColor,
            (int)region.Size.X, fontEntry.Style.Name ?? "regular"
        );

        if (textImage == null)
        {
            return;
        }

        BitmapSkinRenderer.BlitImage(buffer, textImage, tx, ty);
    }

    /// @see TextSkinRenderer.as::isStateDrawable
    public override bool IsStateDrawable(uint state)
    {
        return state == 0;
    }
}
