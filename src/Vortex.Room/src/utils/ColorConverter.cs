using System;

namespace Vortex.Room.Utils;

/// <summary>
/// Static color space conversion utilities (RGB, HSL, XYZ, CIE L*a*b*).
/// All colors are packed as 24-bit int (0xRRGGBB) to match AS3 uint pipeline.
/// </summary>
/// @see com.sulake.room.utils.ColorConverter
public static class ColorConverter
{
    public static int RgbToHSL(int color)
    {
        double r = ((color >> 16) & 0xFF) / 255.0;
        double g = ((color >> 8) & 0xFF) / 255.0;
        double b = (color & 0xFF) / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        double l;

        if (delta == 0)
        {
            h = 0;
        }
        else if (max == r)
        {
            if (g > b)
            {
                h = 60 * (g - b) / delta;
            }
            else
            {
                h = (60 * (g - b) / delta) + 360;
            }
        }
        else if (max == g)
        {
            h = (60 * (b - r) / delta) + 120;
        }
        else if (max == b)
        {
            h = (60 * (r - g) / delta) + 240;
        }

        l = 0.5 * (max + min);

        double s;
        if (delta == 0)
        {
            s = 0;
        }
        else if (l <= 0.5)
        {
            s = delta / l * 0.5;
        }
        else
        {
            s = delta / (1 - l) * 0.5;
        }

        int hInt = (int)Math.Round(h / 360 * 255);
        int sInt = (int)Math.Round(s * 255);
        int lInt = (int)Math.Round(l * 255);

        return (hInt << 16) + (sInt << 8) + lInt;
    }

    public static int HslToRGB(int color)
    {
        double h = ((color >> 16) & 0xFF) / 255.0;
        double s = ((color >> 8) & 0xFF) / 255.0;
        double l = (color & 0xFF) / 255.0;

        double r,
            g,
            b;

        if (s > 0)
        {
            double q;
            if (l < 0.5)
            {
                q = l * (1 + s);
            }
            else
            {
                q = l + s - (l * s);
            }
            double p = (2 * l) - q;

            double tr = h + (1.0 / 3.0);
            double tg = h;
            double tb = h - (1.0 / 3.0);

            switch (tr)
            {
                case < 0:
                    tr += 1;
                    break;
                case > 1:
                    tr -= 1;
                    break;
            }

            switch (tg)
            {
                case < 0:
                    tg += 1;
                    break;
                case > 1:
                    tg -= 1;
                    break;
            }

            switch (tb)
            {
                case < 0:
                    tb += 1;
                    break;
                case > 1:
                    tb -= 1;
                    break;
            }

            r = HslChannel(p, q, tr);
            g = HslChannel(p, q, tg);
            b = HslChannel(p, q, tb);
        }
        else
        {
            r = l;
            g = l;
            b = l;
        }

        int ri = (int)Math.Round(r * 255);
        int gi = (int)Math.Round(g * 255);
        int bi = (int)Math.Round(b * 255);

        return (ri << 16) + (gi << 8) + bi;
    }

    private static double HslChannel(double p, double q, double t)
    {
        if (t * 6 < 1)
        {
            return p + ((q - p) * 6 * t);
        }

        if (t * 2 < 1)
        {
            return q;
        }

        if (t * 3 < 2)
        {
            return p + ((q - p) * 6 * ((2.0 / 3.0) - t));
        }

        return p;
    }

    public static IVector3d Rgb2Xyz(int color)
    {
        double r = ((color >> 16) & 0xFF) / 255.0;
        double g = ((color >> 8) & 0xFF) / 255.0;
        double b = (color & 0xFF) / 255.0;

        r = r > 0.04045 ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
        g = g > 0.04045 ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
        b = b > 0.04045 ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

        r *= 100;
        g *= 100;
        b *= 100;

        return new Vector3d(
            (r * 0.4124) + (g * 0.3576) + (b * 0.1805),
            (r * 0.2126) + (g * 0.7152) + (b * 0.0722),
            (r * 0.0193) + (g * 0.1192) + (b * 0.9505)
        );
    }

    public static IVector3d Xyz2CieLab(IVector3d xyz)
    {
        double x = xyz.X / 95.047;
        double y = xyz.Y / 100.0;
        double z = xyz.Z / 108.883;

        x = x > 0.008856 ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + (16.0 / 116.0);
        y = y > 0.008856 ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + (16.0 / 116.0);
        z = z > 0.008856 ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + (16.0 / 116.0);

        return new Vector3d(
            (116 * y) - 16,
            500 * (x - y),
            200 * (y - z)
        );
    }

    public static IVector3d Rgb2CieLab(int color)
    {
        return Xyz2CieLab(Rgb2Xyz(color));
    }

    public static uint HexToUint(string hex)
    {
        if (hex.StartsWith('#'))
        {
            hex = hex[1..];
        }

        return Convert.ToUInt32(hex, 16);
    }
}
