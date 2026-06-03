// @see core/window/graphics/renderer/BitmapDataRenderer.as

using System;

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/BitmapDataRenderer.as
public class BitmapDataRenderer : SkinRenderer
{
    /// @see BitmapDataRenderer.as::const_412 — reusable color transform for tinting
    private static readonly ColorTransform _colorTransform = new();

    /// @see BitmapDataRenderer.as::const_415 — etching color transform: zero RGB mults, offset replaces color
    private static readonly ColorTransform _etchingTransform = new(0, 0, 0, 1, 1, 1, 1, 0);

    /// @see BitmapDataRenderer.as — greyscale luminance coefficients (ITU-R BT.709)
    private const float GREYSCALE_R = 0.212671f;
    private const float GREYSCALE_G = 0.71516f;
    private const float GREYSCALE_B = 0.072169f;

    /// @see BitmapDataRenderer.as::BitmapDataRenderer
    public BitmapDataRenderer(string name) : base(name) { }

    /// @see BitmapDataRenderer.as::draw
    public override void Draw(IWindow window, Image buffer, Rect2 region, uint state, bool flag)
    {
        if (window is not IBitmapDataContainer bitmapContainer)
        {
            return;
        }
        if (buffer == null)
        {
            return;
        }

        if (bitmapContainer.BitmapData is not Image bitmapData)
        {
            return;
        }

        // Read container properties
        float rotation = bitmapContainer.Rotation;
        bool stretchedX = bitmapContainer.StretchedX;
        bool stretchedY = bitmapContainer.StretchedY;
        float zoomX = bitmapContainer.ZoomX;
        float zoomY = bitmapContainer.ZoomY;
        bool wrapX = bitmapContainer.WrapX;
        bool wrapY = bitmapContainer.WrapY;
        int pivotPoint = (int)bitmapContainer.PivotPoint;
        bool greyscale = bitmapContainer.Greyscale;
        uint etchingColor = bitmapContainer.EtchingColor;

        // Etching point
        int etchPointX = (int)bitmapContainer.EtchingPoint.X;
        int etchPointY = (int)bitmapContainer.EtchingPoint.Y;

        // @see BitmapDataRenderer.as — apply rotation if needed
        Image sourceImage = bitmapData;
        if (rotation != 0)
        {
            sourceImage = RotateImage(bitmapData, rotation);
        }

        int srcW = sourceImage.GetWidth();
        int srcH = sourceImage.GetHeight();

        // @see BitmapDataRenderer.as — compute scaled dimensions
        int scaledW = (int)((stretchedX ? window.width : srcW) * zoomX);
        int scaledH = (int)((stretchedY ? window.height : srcH) * zoomY);

        if (scaledW <= 0 || scaledH <= 0)
        {
            return;
        }

        // @see BitmapDataRenderer.as — compute tile counts for wrapping
        int tilesX = wrapX ? (int)(window.width / scaledW) + 2 : 1;
        int tilesY = wrapY ? (int)(window.height / scaledH) + 2 : 1;

        // @see BitmapDataRenderer.as — compute pivot-based origin X
        // Pivot grid: 0=TL, 1=TC, 2=TR, 3=ML, 4=MC, 5=MR, 6=BL, 7=BC, 8=BR
        int pivotCol = pivotPoint % 3;
        int originX = pivotCol switch
        {
            0 => // left column (0, 3, 6)
                zoomX < 0 ? -scaledW : 0,
            1 => // center column (1, 4, 7)
                (int)((window.width - scaledW) / 2),
            _ => zoomX < 0 ? (int)window.width : (int)window.width - scaledW,
        };

        // @see BitmapDataRenderer.as — wrap X: shift origin left until < 0
        int wrapStartX = originX;
        while (wrapX && wrapStartX > 0)
        {
            wrapStartX -= scaledW;
        }

        // @see BitmapDataRenderer.as — compute pivot-based origin Y
        int pivotRow = pivotPoint / 3;
        int originY = pivotRow switch
        {
            0 => // top row (0, 1, 2)
                zoomY < 0 ? -scaledH : 0,
            1 => // middle row (3, 4, 5)
                (int)((window.height - scaledH) / 2),
            _ => zoomY < 0 ? (int)window.height : (int)window.height - scaledH,
        };

        // @see BitmapDataRenderer.as — wrap Y: shift origin up until < 0
        int wrapStartY = originY;
        while (wrapY && wrapStartY > 0)
        {
            wrapStartY -= scaledH;
        }

        // @see BitmapDataRenderer.as — compute color tinting from window color
        float rMult = ((window.color & 0xFF0000) >> 16) / 255f;
        float gMult = ((window.color & 0x00FF00) >> 8) / 255f;
        float bMult = (window.color & 0x0000FF) / 255f;

        // @see BitmapDataRenderer.as — setup etching transform
        float etchAlpha = ((etchingColor >> 24) & 0xFF) / 255f;
        byte etchR = (byte)((etchingColor >> 16) & 0xFF);
        byte etchG = (byte)((etchingColor >> 8) & 0xFF);
        byte etchB = (byte)(etchingColor & 0xFF);

        // Pre-scale the source image if needed
        Image? scaledImage = sourceImage;
        if (scaledW != srcW || scaledH != srcH)
        {
            scaledImage = (Image)sourceImage.Duplicate();
            scaledImage.Resize(scaledW, scaledH);
        }

        // @see BitmapDataRenderer.as — tile loop
        int drawY = wrapStartY;

        for (int ty = 0;
             ty < tilesY;
             ty++)
        {
            int drawX = wrapStartX;

            for (int tx = 0;
                 tx < tilesX;
                 tx++)
            {
                if (greyscale)
                {
                    // @see BitmapDataRenderer.as — greyscale path: draw with luminance filter
                    // Draw etching first if alpha > 0
                    if (etchAlpha >= 0.001f)
                    {
                        DrawImageWithEtching(
                            buffer, scaledImage,
                            drawX + etchPointX, drawY + etchPointY,
                            etchR / 255f, etchG / 255f, etchB / 255f, etchAlpha
                        );
                    }
                    // Draw greyscale-tinted image
                    DrawGreyscale(buffer, scaledImage, drawX, drawY, rMult, gMult, bMult);
                }
                else
                {
                    // @see BitmapDataRenderer.as — normal path with color tinting
                    float finalRMult = rMult;
                    float finalGMult = gMult;
                    float finalBMult = bMult;
                    float finalAMult = 1f;
                    float finalROff = 0f;
                    float finalGOff = 0f;
                    float finalBOff = 0f;
                    float finalAOff = 0f;

                    // @see BitmapDataRenderer.as — concat dynamicStyleColor if present
                    if (window.dynamicStyleColor != null)
                    {
                        ColorTransform? dsc = window.dynamicStyleColor;
                        // Flash ColorTransform.concat: new = old * other
                        finalRMult *= dsc.RedMultiplier;
                        finalGMult *= dsc.GreenMultiplier;
                        finalBMult *= dsc.BlueMultiplier;
                        finalAMult *= dsc.AlphaMultiplier;
                        finalROff = (finalROff * dsc.RedMultiplier) + dsc.RedOffset;
                        finalGOff = (finalGOff * dsc.GreenMultiplier) + dsc.GreenOffset;
                        finalBOff = (finalBOff * dsc.BlueMultiplier) + dsc.BlueOffset;
                        finalAOff = (finalAOff * dsc.AlphaMultiplier) + dsc.AlphaOffset;
                    }

                    // Draw etching first if alpha > 0
                    if (etchAlpha >= 0.001f)
                    {
                        DrawImageWithEtching(
                            buffer, scaledImage,
                            drawX + etchPointX, drawY + etchPointY,
                            etchR / 255f, etchG / 255f, etchB / 255f, etchAlpha
                        );
                    }

                    // Draw tinted image
                    DrawTinted(
                        buffer, scaledImage, drawX, drawY,
                        finalRMult, finalGMult, finalBMult, finalAMult,
                        finalROff, finalGOff, finalBOff, finalAOff
                    );
                }

                drawX += scaledW;
            }
            drawY += scaledH;
        }
    }

    /// @see BitmapDataRenderer.as::isStateDrawable
    public override bool IsStateDrawable(uint state)
    {
        return state == 0;
    }

    /// <summary>
    /// Rotates an Image by the given degrees around its center.
    /// Godot adaptation of AS3 Matrix.rotate + BitmapData.draw.
    /// </summary>
    private static Image RotateImage(Image source, float degrees)
    {
        int w = source.GetWidth();
        int h = source.GetHeight();
        float cx = w / 2f;
        float cy = h / 2f;
        float rad = degrees / 180f * Mathf.Pi;
        float cos = Mathf.Cos(-rad);
        float sin = Mathf.Sin(-rad);

        Image? result = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);

        for (int y = 0;
             y < h;
             y++)
        {
            for (int x = 0;
                 x < w;
                 x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                int srcX = (int)((cos * dx) - (sin * dy) + cx);
                int srcY = (int)((sin * dx) + (cos * dy) + cy);

                if (srcX >= 0 && srcX < w && srcY >= 0 && srcY < h)
                {
                    result.SetPixel(x, y, source.GetPixel(srcX, srcY));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Draws source image onto buffer with color multiplier tinting and offsets.
    /// </summary>
    private static void DrawTinted
    (
        Image dest, Image source, int destX, int destY,
        float rMult, float gMult, float bMult, float aMult,
        float rOff, float gOff, float bOff, float aOff
    )
    {
        int srcW = source.GetWidth();
        int srcH = source.GetHeight();
        int dstW = dest.GetWidth();
        int dstH = dest.GetHeight();

        for (int y = 0;
             y < srcH;
             y++)
        {
            int dy = destY + y;

            if (dy < 0 || dy >= dstH)
            {
                continue;
            }

            for (int x = 0;
                 x < srcW;
                 x++)
            {
                int dx = destX + x;

                if (dx < 0 || dx >= dstW)
                {
                    continue;
                }

                Color srcPixel = source.GetPixel(x, y);

                if (!(srcPixel.A > 0))
                {
                    continue;
                }

                float r = Math.Clamp((srcPixel.R * rMult) + (rOff / 255f), 0f, 1f);
                float g = Math.Clamp((srcPixel.G * gMult) + (gOff / 255f), 0f, 1f);
                float b = Math.Clamp((srcPixel.B * bMult) + (bOff / 255f), 0f, 1f);
                float a = Math.Clamp((srcPixel.A * aMult) + (aOff / 255f), 0f, 1f);

                if (a <= 0)
                {
                    continue;
                }

                Color tinted = new(r, g, b, a);

                if (a >= 1f)
                {
                    dest.SetPixel(dx, dy, tinted);
                }
                else
                {
                    Color dstPixel = dest.GetPixel(dx, dy);
                    float da = dstPixel.A * (1f - a);
                    float oa = a + da;

                    if (oa > 0)
                    {
                        dest.SetPixel(
                            dx, dy, new Color(
                                ((tinted.R * a) + (dstPixel.R * da)) / oa,
                                ((tinted.G * a) + (dstPixel.G * da)) / oa,
                                ((tinted.B * a) + (dstPixel.B * da)) / oa,
                                oa
                            )
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws source image onto buffer with greyscale luminance tinting.
    /// </summary>
    private static void DrawGreyscale
    (
        Image dest, Image source, int destX, int destY,
        float rMult, float gMult, float bMult
    )
    {
        int srcW = source.GetWidth();
        int srcH = source.GetHeight();
        int dstW = dest.GetWidth();
        int dstH = dest.GetHeight();

        for (int y = 0;
             y < srcH;
             y++)
        {
            int dy = destY + y;
            if (dy < 0 || dy >= dstH)
            {
                continue;
            }

            for (int x = 0;
                 x < srcW;
                 x++)
            {
                int dx = destX + x;

                if (dx < 0 || dx >= dstW)
                {
                    continue;
                }

                Color srcPixel = source.GetPixel(x, y);

                if (!(srcPixel.A > 0))
                {
                    continue;
                }

                // @see BitmapDataRenderer.as — greyscale matrix applies luminance per channel
                float grey = (srcPixel.R * GREYSCALE_R) + (srcPixel.G * GREYSCALE_G) + (srcPixel.B * GREYSCALE_B);
                float r = Math.Clamp(grey * rMult, 0f, 1f);
                float g = Math.Clamp(grey * gMult, 0f, 1f);
                float b = Math.Clamp(grey * bMult, 0f, 1f);

                Color tinted = new(r, g, b, srcPixel.A);

                if (srcPixel.A >= 1f)
                {
                    dest.SetPixel(dx, dy, tinted);
                }
                else
                {
                    Color dstPixel = dest.GetPixel(dx, dy);
                    float sa = srcPixel.A;
                    float da = dstPixel.A * (1f - sa);
                    float oa = sa + da;

                    if (oa > 0)
                    {
                        dest.SetPixel(
                            dx, dy, new Color(
                                ((tinted.R * sa) + (dstPixel.R * da)) / oa,
                                ((tinted.G * sa) + (dstPixel.G * da)) / oa,
                                ((tinted.B * sa) + (dstPixel.B * da)) / oa,
                                oa
                            )
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws source image with flat etching color (replaces RGB, keeps alpha shape).
    /// </summary>
    private static void DrawImageWithEtching
    (
        Image dest, Image source,
        int destX, int destY, float etchR, float etchG, float etchB, float etchAlpha
    )
    {
        int srcW = source.GetWidth();
        int srcH = source.GetHeight();
        int dstW = dest.GetWidth();
        int dstH = dest.GetHeight();

        for (int y = 0;
             y < srcH;
             y++)
        {
            int dy = destY + y;
            if (dy < 0 || dy >= dstH)
            {
                continue;
            }

            for (int x = 0;
                 x < srcW;
                 x++)
            {
                int dx = destX + x;
                if (dx < 0 || dx >= dstW)
                {
                    continue;
                }

                Color srcPixel = source.GetPixel(x, y);

                if (!(srcPixel.A > 0))
                {
                    continue;
                }

                float a = srcPixel.A * etchAlpha;

                if (a <= 0)
                {
                    continue;
                }

                Color etchPixel = new(etchR, etchG, etchB, a);

                if (a >= 1f)
                {
                    dest.SetPixel(dx, dy, etchPixel);
                }
                else
                {
                    Color dstPixel = dest.GetPixel(dx, dy);
                    float da = dstPixel.A * (1f - a);
                    float oa = a + da;
                    if (oa > 0)
                    {
                        dest.SetPixel(
                            dx, dy, new Color(
                                ((etchR * a) + (dstPixel.R * da)) / oa,
                                ((etchG * a) + (dstPixel.G * da)) / oa,
                                ((etchB * a) + (dstPixel.B * da)) / oa,
                                oa
                            )
                        );
                    }
                }
            }
        }
    }
}
