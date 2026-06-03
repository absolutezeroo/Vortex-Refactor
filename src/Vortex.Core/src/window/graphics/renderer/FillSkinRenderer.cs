// @see core/window/graphics/renderer/FillSkinRenderer.as

using Godot;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/FillSkinRenderer.as
public class FillSkinRenderer : SkinRenderer
{
    /// @see FillSkinRenderer.as::FillSkinRenderer
    public FillSkinRenderer(string name) : base(name) { }

    /// @see FillSkinRenderer.as::draw
    /// Godot optimization: replaced per-pixel SetPixel loop with Image.FillRect.
    public override void Draw(IWindow window, Image buffer, Rect2 region, uint state, bool flag)
    {
        uint color = window.color;
        byte a = (byte)((color >> 24) & 0xFF);
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);

        // @see FillSkinRenderer.as — alpha 0 = transparent fill, do NOT force to 0xFF
        Color godotColor = new(r / 255f, g / 255f, b / 255f, a / 255f);

        int x0 = System.Math.Max(0, (int)region.Position.X);
        int y0 = System.Math.Max(0, (int)region.Position.Y);
        int x1 = System.Math.Min(buffer.GetWidth(), (int)region.End.X);
        int y1 = System.Math.Min(buffer.GetHeight(), (int)region.End.Y);

        if (x1 <= x0 || y1 <= y0)
        {
            return;
        }

        buffer.FillRect(new Rect2I(x0, y0, x1 - x0, y1 - y0), godotColor);
    }
}
