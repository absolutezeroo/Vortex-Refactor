using Godot;

namespace Vortex.Room.Utils;

/// @see com.sulake.room.utils.class_3817
public static class BitmapDataUtil
{
    public static void Line(Image bitmap, Vector2I start, Vector2I end, int color)
    {
        int dx = end.X - start.X;
        int dy = end.Y - start.Y;
        int sx = dx > 0 ? 1 : -1;
        int sy = dy > 0 ? 1 : -1;
        dx = System.Math.Abs(dx);
        dy = System.Math.Abs(dy);

        int x = start.X;
        int y = start.Y;

        SetPixel32(bitmap, x, y, color);

        if (dx == 0 && dy == 0)
        {
            return;
        }

        int err = 0;

        if (dx > dy)
        {
            for (int i = dx - 1; i >= 0; i--)
            {
                err += dy;
                x += sx;

                if (err >= dx / 2)
                {
                    err -= dx;
                    y += sy;
                }

                SetPixel32(bitmap, x, y, color);
            }
        }
        else
        {
            for (int i = dy - 1; i >= 0; i--)
            {
                err += dx;
                y += sy;

                if (err >= dy / 2)
                {
                    err -= dy;
                    x += sx;
                }

                SetPixel32(bitmap, x, y, color);
            }
        }

        SetPixel32(bitmap, end.X, end.Y, color);
    }

    public static Image? GetFlipHBitmapData(Image? source)
    {
        if (source == null)
        {
            return null;
        }

        int w = source.GetWidth();
        int h = source.GetHeight();
        Image? result = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                result.SetPixel(w - 1 - x, y, source.GetPixel(x, y));
            }
        }

        return result;
    }

    public static Image? GetFlipVBitmapData(Image? source)
    {
        if (source == null)
        {
            return null;
        }

        int w = source.GetWidth();
        int h = source.GetHeight();
        Image? result = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                result.SetPixel(x, h - 1 - y, source.GetPixel(x, y));
            }
        }

        return result;
    }

    public static Image? GetFlipHVBitmapData(Image? source)
    {
        if (source == null)
        {
            return null;
        }

        int w = source.GetWidth();
        int h = source.GetHeight();
        Image? result = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                result.SetPixel(w - 1 - x, h - 1 - y, source.GetPixel(x, y));
            }
        }
        return result;
    }

    private static void SetPixel32(Image bitmap, int x, int y, int color)
    {
        if (x < 0 || x >= bitmap.GetWidth() || y < 0 || y >= bitmap.GetHeight())
        {
            return;
        }

        float a = ((color >> 24) & 0xFF) / 255f;
        float r = ((color >> 16) & 0xFF) / 255f;
        float g = ((color >> 8) & 0xFF) / 255f;
        float b = (color & 0xFF) / 255f;
        bitmap.SetPixel(x, y, new Color(r, g, b, a));
    }
}
