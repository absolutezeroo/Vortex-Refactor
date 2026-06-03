// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AvatarDataContainer.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Animation;

/// <summary>
/// Holds avatar-level animation overrides: ink mode, color transform, and palette map.
/// Godot adaptation: Flash ColorTransform mapped to float[4] (r,g,b,a multipliers).
/// Palette map stored as int[256] arrays for grayscale-to-color remapping.
/// </summary>
/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AvatarDataContainer.as
public class AvatarDataContainer : IAvatarSpriteData
{
    private readonly int[] _reds;
    private readonly int[] _greens;
    private readonly int[] _blues;
    private readonly int[] _alphas;

    /// @see AvatarDataContainer.as::AvatarDataContainer
    public AvatarDataContainer(XElement xml)
    {
        Ink = (int?)xml.Attribute("ink") ?? 0;

        string fgStr = ((string?)xml.Attribute("foreground") ?? "").Replace("#", "");
        string bgStr = ((string?)xml.Attribute("background") ?? "").Replace("#", "");

        uint foreground = System.Convert.ToUInt32(fgStr.Length > 0 ? fgStr : "0", 16);
        uint background = System.Convert.ToUInt32(bgStr.Length > 0 ? bgStr : "0", 16);

        uint colorRgb = foreground;
        uint r = (colorRgb >> 16) & 0xFF;
        uint g = (colorRgb >> 8) & 0xFF;
        uint b = colorRgb & 0xFF;

        float rMul = r / 255f;
        float gMul = g / 255f;
        float bMul = b / 255f;
        float aMul = 1f;

        PaletteIsGrayscale = true;

        // AS3: ink == 37 → alpha = 0.5, paletteIsGrayscale = false
        if (Ink == 37)
        {
            aMul = 0.5f;
            PaletteIsGrayscale = false;
        }

        ColorTransform = [rMul, gMul, bMul, aMul];

        GeneratePaletteMapForGrayscale(
            background, foreground,
            out _reds, out _greens, out _blues, out _alphas
        );
    }

    /// @see AvatarDataContainer.as::generatePaletteMapForGrayscale
    private static void GeneratePaletteMapForGrayscale
    (
        uint background,
        uint foreground,
        out int[] reds,
        out int[] greens,
        out int[] blues,
        out int[] alphas
    )
    {
        int bgA = (int)((background >> 24) & 0xFF);
        int bgR = (int)((background >> 16) & 0xFF);
        int bgG = (int)((background >> 8) & 0xFF);
        int bgB = (int)(background & 0xFF);

        int fgA = (int)((foreground >> 24) & 0xFF);
        int fgR = (int)((foreground >> 16) & 0xFF);
        int fgG = (int)((foreground >> 8) & 0xFF);
        int fgB = (int)(foreground & 0xFF);

        double aStep = (fgA - bgA) / 255.0;
        double rStep = (fgR - bgR) / 255.0;
        double gStep = (fgG - bgG) / 255.0;
        double bStep = (fgB - bgB) / 255.0;

        reds = new int[256];
        greens = new int[256];
        blues = new int[256];
        alphas = new int[256];

        double a = bgA;
        double rv = bgR;
        double gv = bgG;
        double bv = bgB;

        for (int i = 0;
             i < 256;
             i++)
        {
            // AS3: first iteration zeroes alpha when rgb matches background
            if ((int)rv == bgR && (int)gv == bgG && (int)bv == bgB)
            {
                a = 0;
            }

            a += aStep;
            rv += rStep;
            gv += gStep;
            bv += bStep;

            int ai = (int)a << 24;
            int ri = (int)rv;
            int gi = (int)gv;
            int bi = (int)bv;

            int combined = ai | (ri << 16) | (gi << 8) | bi;

            alphas[i] = combined;
            reds[i] = combined;
            greens[i] = combined;
            blues[i] = combined;
        }
    }

    /// @see AvatarDataContainer.as::get ink
    public int Ink { get; }

    /// @see AvatarDataContainer.as::get colorTransform
    public float[] ColorTransform { get; }

    /// @see AvatarDataContainer.as::get paletteIsGrayscale
    public bool PaletteIsGrayscale { get; }

    /// @see AvatarDataContainer.as::get reds
    public int[] Reds => _reds;

    /// @see AvatarDataContainer.as::get greens
    public int[] Greens => _greens;

    /// @see AvatarDataContainer.as::get blues
    public int[] Blues => _blues;

    /// @see AvatarDataContainer.as::get alphas
    public int[] Alphas => _alphas;
}
