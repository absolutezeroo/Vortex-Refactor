using Godot;

using Vortex.Room.Utils;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneVisualizationLayer
public class PlaneVisualizationLayer(PlaneMaterial? material, uint color, int align, int offset = 0)
    : IDisposable
{
    public const int DEFAULT_OFFSET = 0;
    public const int ALIGN_TOP = 1;
    public const int ALIGN_BOTTOM = 2;
    public const int ALIGN_DEFAULT = 1;

    private PlaneMaterial? _material = material;
    private Image? _bitmapData;

    public int Offset => offset;

    public int Align => align;

    public bool Disposed { get; private set; }

    bool Core.Runtime.IDisposable.disposed => Disposed;

    public void Dispose()
    {
        Disposed = true;
        _material = null;
        _bitmapData = null;
    }

    public void ClearCache()
    {
        _bitmapData = null;
    }

    public Image? Render(Image? canvas, int width, int height, IVector3d normal, bool useTexture, int offsetX, int offsetY)
    {
        uint r = (color >> 16) & 0xFF;
        uint g = (color >> 8) & 0xFF;
        uint b = color & 0xFF;
        bool needsColorTransform = r < 255 || g < 255 || b < 255;

        if (canvas != null && (canvas.GetWidth() != width || canvas.GetHeight() != height))
        {
            canvas = null;
        }

        Image? result = null;

        if (_material != null)
        {
            if (needsColorTransform)
            {
                result = _material.Render(null, width, height, normal, useTexture, offsetX, offsetY + Offset, Align == ALIGN_TOP);
            }
            else
            {
                result = _material.Render(canvas, width, height, normal, useTexture, offsetX, offsetY + Offset, Align == ALIGN_TOP);
            }

            if (result != null && result != canvas)
            {
                _bitmapData = (Image)result.Duplicate();
                result = _bitmapData;
            }
        }
        else if (canvas == null)
        {
            if (_bitmapData != null && _bitmapData.GetWidth() == width && _bitmapData.GetHeight() == height)
            {
                return _bitmapData;
            }

            _bitmapData = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
            _bitmapData.Fill(Colors.White);
            result = _bitmapData;
        }
        else
        {
            canvas.Fill(Colors.White);
            result = canvas;
        }

        if (result == null)
        {
            return null;
        }

        if (!needsColorTransform)
        {
            return result;
        }

        float rMul = r / 255f;
        float gMul = g / 255f;
        float bMul = b / 255f;

        int w = result.GetWidth();
        int h = result.GetHeight();

        for (int py = 0; py < h; py++)
        {
            for (int px = 0; px < w; px++)
            {
                Color pixel = result.GetPixel(px, py);

                if (pixel.A <= 0f)
                {
                    continue;
                }

                result.SetPixel(px, py, new Color(pixel.R * rMul, pixel.G * gMul, pixel.B * bMul, pixel.A));
            }
        }

        if (canvas == null || result == canvas)
        {
            return result;
        }

        BlitWithAlpha(canvas, result, new Rect2I(0, 0, w, h), Vector2I.Zero);

        result = canvas;

        return result;
    }

    public PlaneMaterial? GetMaterial()
    {
        return _material;
    }

    public uint GetColor()
    {
        return color;
    }

    private static void BlitWithAlpha(Image dest, Image src, Rect2I srcRect, Vector2I destPos)
    {
        int sw = src.GetWidth();
        int sh = src.GetHeight();
        int dw = dest.GetWidth();
        int dh = dest.GetHeight();

        for (int y = 0; y < srcRect.Size.Y; y++)
        {
            int sy = srcRect.Position.Y + y;
            int dy = destPos.Y + y;

            if (sy < 0 || sy >= sh || dy < 0 || dy >= dh)
            {
                continue;
            }

            for (int x = 0; x < srcRect.Size.X; x++)
            {
                int sx = srcRect.Position.X + x;
                int dx = destPos.X + x;

                if (sx < 0 || sx >= sw || dx < 0 || dx >= dw)
                {
                    continue;
                }

                Color srcColor = src.GetPixel(sx, sy);

                switch (srcColor.A)
                {
                    case <= 0f:
                        continue;
                    case >= 1f:
                        dest.SetPixel(dx, dy, srcColor);

                        break;
                    default:
                        {
                            Color dstColor = dest.GetPixel(dx, dy);
                            float outA = srcColor.A + dstColor.A * (1f - srcColor.A);

                            if (outA > 0f)
                            {
                                float cr = (srcColor.R * srcColor.A + dstColor.R * dstColor.A * (1f - srcColor.A)) / outA;
                                float cg = (srcColor.G * srcColor.A + dstColor.G * dstColor.A * (1f - srcColor.A)) / outA;
                                float cb = (srcColor.B * srcColor.A + dstColor.B * dstColor.A * (1f - srcColor.A)) / outA;

                                dest.SetPixel(dx, dy, new Color(cr, cg, cb, outA));
                            }

                            break;
                        }
                }

            }
        }
    }
}
