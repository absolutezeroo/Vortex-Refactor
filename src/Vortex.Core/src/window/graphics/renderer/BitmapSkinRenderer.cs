// @see core/window/graphics/renderer/BitmapSkinRenderer.as

using System;

using Godot;

namespace Vortex.Core.Window.Graphics.Renderer;

/// @see core/window/graphics/renderer/BitmapSkinRenderer.as
public class BitmapSkinRenderer : SkinRenderer
{
    protected Dictionary<string, Image> _bitmapCache;
    protected ColorTransform _colorTransform;

    /// @see BitmapSkinRenderer.as::BitmapSkinRenderer
    public BitmapSkinRenderer(string name) : base(name)
    {
        _bitmapCache = new Dictionary<string, Image>();
        _colorTransform = new ColorTransform();
    }

    /// @see BitmapSkinRenderer.as::parse
    public override void Parse(System.Xml.Linq.XElement? skinXml, System.Xml.Linq.XElement? xmlStates, Func<string, Image?>? assetResolver)
    {
        if (skinXml != null)
        {
            Class3725.ParseSkinDescription(skinXml, xmlStates, this, Name, assetResolver);
        }
    }

    /// @see BitmapSkinRenderer.as::dispose
    public override void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        base.Dispose();

        _colorTransform = null!;
        _bitmapCache?.Clear();
        _bitmapCache = null!;
    }

    /// @see BitmapSkinRenderer.as::isStateDrawable
    public override bool IsStateDrawable(uint state)
    {
        return GetTemplateByState(state) != null;
    }

    /// @see BitmapSkinRenderer.as::draw
    public override void Draw(IWindow window, Image buffer, Rect2 region, uint state, bool flag)
    {
        ISkinLayout? layout = GetLayoutByState(state);
        ISkinTemplate? template = GetTemplateByState(state);
        if (layout == null)
        {
            layout = GetLayoutByState(0);
            template = GetTemplateByState(0);
        }

        int numEntities = layout?.NumChildren ?? 0;

        if (layout == null || numEntities == 0)
        {
            return;
        }

        int widthDelta = (int)region.Size.X - (int)layout.Width;
        int heightDelta = (int)region.Size.Y - (int)layout.Height;

        bool colorize = !window.background && (window.color & 0xFFFFFF) < 0xFFFFFF;

        if (colorize)
        {
            _colorTransform.RedMultiplier = ((window.color & 0xFF0000) >> 16) / 255f;
            _colorTransform.GreenMultiplier = ((window.color & 0x00FF00) >> 8) / 255f;
            _colorTransform.BlueMultiplier = (window.color & 0x0000FF) / 255f;
            _colorTransform.AlphaMultiplier = 1f;
        }

        for (int i = 0;
             i < numEntities;
             i++)
        {
            if (layout.GetChildAt(i) is not SkinLayoutEntity layoutEntity)
            {
                continue;
            }

            if (template?.GetChildByName(layoutEntity.Name) is not ISkinTemplateEntity)
            {
                continue;
            }

            Image? cachedBitmap = GetBitmapFromCache(template!, layoutEntity.Name);

            if (cachedBitmap == null)
            {
                continue;
            }

            Image sourceBitmap = cachedBitmap;
            bool needsDispose = false;

            if (colorize && layoutEntity.Colorize)
            {
                sourceBitmap = ApplyColorTransform(cachedBitmap, _colorTransform);
                needsDispose = true;
            }

            bool stretchH = false;
            bool stretchV = false;

            float destX = layoutEntity.Region.Position.X + region.Position.X;
            float destY = layoutEntity.Region.Position.Y + region.Position.Y;
            float destW = layoutEntity.Region.Size.X;
            float destH = layoutEntity.Region.Size.Y;

            switch (layoutEntity.ScaleH)
            {
                // Horizontal scale type
                case SkinLayoutEntity.SCALE_TYPE_MOVE:
                    destX += widthDelta;
                    break;
                case SkinLayoutEntity.SCALE_TYPE_STRECH:
                case SkinLayoutEntity.SCALE_TYPE_TILED:
                    stretchH = true;
                    destW += widthDelta;
                    break;
                case SkinLayoutEntity.SCALE_TYPE_CENTER:
                    destX = (region.Size.X / 2f) - (destW / 2f);
                    break;
            }

            // @see BitmapSkinRenderer.as — AS3 breaks entire entity loop when stretched dimension < 1
            if (stretchH && destW < 1)
            {
                if (needsDispose)
                {
                    sourceBitmap.Dispose();
                }
                break; // exit entity loop entirely, matching AS3
            }

            switch (layoutEntity.ScaleV)
            {
                // Vertical scale type
                case SkinLayoutEntity.SCALE_TYPE_MOVE:
                    destY += heightDelta;
                    break;
                case SkinLayoutEntity.SCALE_TYPE_STRECH:
                case SkinLayoutEntity.SCALE_TYPE_TILED:
                    stretchV = true;
                    destH += heightDelta;
                    break;
                case SkinLayoutEntity.SCALE_TYPE_CENTER:
                    destY = (region.Size.Y / 2f) - (destH / 2f);
                    break;
            }

            // @see BitmapSkinRenderer.as — AS3 breaks entire entity loop when stretched dimension < 1
            if (stretchV && destH < 1)
            {
                if (needsDispose)
                {
                    sourceBitmap.Dispose();
                }
                break; // exit entity loop entirely, matching AS3
            }

            // Draw
            if (!stretchH && !stretchV)
            {
                // Simple copy
                BlitImage(buffer, sourceBitmap, (int)destX, (int)destY);
            }
            else if (layoutEntity.ScaleV == SkinLayoutEntity.SCALE_TYPE_TILED ||
                     layoutEntity.ScaleH == SkinLayoutEntity.SCALE_TYPE_TILED)
            {
                // Tiled rendering
                TileBlit(buffer, sourceBitmap, (int)destX, (int)destY, (int)destW, (int)destH);
            }
            else
            {
                // Stretched rendering
                if (sourceBitmap.GetWidth() == 1 && sourceBitmap.GetHeight() == 1)
                {
                    // 1x1 pixel - just fill
                    Color pixel = sourceBitmap.GetPixel(0, 0);

                    FillRegion(buffer, (int)destX, (int)destY, (int)destW, (int)destH, pixel);
                }
                else if (destW >= 1 && destH >= 1)
                {
                    // Scale and blit
                    Image scaled = ScaleImage(sourceBitmap, (int)destW, (int)destH);
                    BlitImage(buffer, scaled, (int)destX, (int)destY);
                }
            }

            // @see BitmapSkinRenderer.as line 246-249 — dispose colorized clone after drawing entity
            if (needsDispose)
            {
                sourceBitmap.Dispose();
            }
        }
    }

    /// @see BitmapSkinRenderer.as::drawStaticLayoutEntity
    protected void DrawStaticLayoutEntity
    (
        Image buffer,
        Rect2 drawRegion,
        ISkinLayout layout,
        SkinLayoutEntity layoutEntity,
        ISkinTemplate template,
        ISkinTemplateEntity templateEntity
    )
    {
        float x = layoutEntity.Region.Position.X + drawRegion.Position.X;
        float y = layoutEntity.Region.Position.Y + drawRegion.Position.Y;

        switch (templateEntity.Type)
        {
            case "bitmap":
                Image? bitmap = GetBitmapFromCache(template, layoutEntity.Name);

                if (bitmap == null)
                {
                    return;
                }

                if (layoutEntity.ScaleH == SkinLayoutEntity.SCALE_TYPE_MOVE)
                {
                    x += drawRegion.Size.X - layout.Width;
                }

                if (layoutEntity.ScaleV == SkinLayoutEntity.SCALE_TYPE_MOVE)
                {
                    y += drawRegion.Size.Y - layout.Height;
                }

                BlitImage(buffer, bitmap, (int)x, (int)y);
                break;
            case "fill":
                uint color = layoutEntity.Color;
                byte a = (byte)((color >> 24) & 0xFF);
                byte r = (byte)((color >> 16) & 0xFF);
                byte g = (byte)((color >> 8) & 0xFF);
                byte b = (byte)(color & 0xFF);
                float alpha = a / 255f;

                FillRegion(
                    buffer, (int)x, (int)y,
                    (int)layoutEntity.Region.Size.X, (int)layoutEntity.Region.Size.Y,
                    new Color(r / 255f, g / 255f, b / 255f, alpha)
                );
                break;
        }
    }

    /// @see BitmapSkinRenderer.as::getBitmapFromCache
    protected Image? GetBitmapFromCache(ISkinTemplate template, string entityName)
    {
        string key = entityName + "@" + template.Name;

        if (_bitmapCache.TryGetValue(key, out Image? cached))
        {
            return cached;
        }

        if (template.GetChildByName(entityName) is not ISkinTemplateEntity entity)
        {
            throw new Exception($"Template entity {entityName} not found!");
        }

        Image? sourceAsset = template.Asset ?? throw new Exception($"Asset not found for template!");

        Rect2 entityRegion = entity.Region;
        int srcX = (int)entityRegion.Position.X;
        int srcY = (int)entityRegion.Position.Y;
        int srcW = (int)entityRegion.Size.X;
        int srcH = (int)entityRegion.Size.Y;

        if (srcW <= 0 || srcH <= 0)
        {
            return null;
        }

        // @see Godot BlitRect requires matching formats; ensure source is Rgba8
        if (sourceAsset.GetFormat() != Image.Format.Rgba8)
        {
            sourceAsset.Convert(Image.Format.Rgba8);
        }

        Image? cropped = Image.CreateEmpty(srcW, srcH, false, Image.Format.Rgba8);
        cropped.BlitRect(sourceAsset, new Rect2I(srcX, srcY, srcW, srcH), Vector2I.Zero);

        _bitmapCache[key] = cropped;

        return cropped;
    }

    /// Godot adaptation: uses Image.BlendRect for hardware-accelerated alpha compositing
    /// instead of per-pixel GetPixel/SetPixel loops.
    /// @see AS3 copyPixels(source, rect, point, null, null, mergeAlpha=true)
    internal static void BlitImage(Image dest, Image source, int destX, int destY)
    {
        dest.BlendRect(
            source,
            new Rect2I(0, 0, source.GetWidth(), source.GetHeight()),
            new Vector2I(destX, destY)
        );
    }

    /// Godot adaptation: uses BlendRect with clipped source rects instead of
    /// allocating temporary partial images for edge tiles.
    private static void TileBlit(Image dest, Image source, int destX, int destY, int destW, int destH)
    {
        int srcW = source.GetWidth();
        int srcH = source.GetHeight();

        if (srcW <= 0 || srcH <= 0)
        {
            return;
        }

        for (int oy = 0; oy < destH; oy += srcH)
        {
            int h = Math.Min(srcH, destH - oy);

            for (int ox = 0; ox < destW; ox += srcW)
            {
                int w = Math.Min(srcW, destW - ox);
                dest.BlendRect(source, new Rect2I(0, 0, w, h), new Vector2I(destX + ox, destY + oy));
            }
        }
    }

    private static Image ScaleImage(Image source, int newWidth, int newHeight)
    {
        Image? copy = (Image)source.Duplicate();

        copy.Resize(newWidth, newHeight);

        return copy;
    }

    /// Godot adaptation: uses Image.FillRect for hardware-accelerated region fill.
    internal static void FillRegion(Image dest, int x, int y, int w, int h, Color color)
    {
        int dstW = dest.GetWidth();
        int dstH = dest.GetHeight();
        int x0 = Math.Max(0, x);
        int y0 = Math.Max(0, y);
        int x1 = Math.Min(x + w, dstW);
        int y1 = Math.Min(y + h, dstH);

        if (x1 <= x0 || y1 <= y0)
        {
            return;
        }

        dest.FillRect(new Rect2I(x0, y0, x1 - x0, y1 - y0), color);
    }

    /// Godot optimization: replaced per-pixel GetPixel/SetPixel with bulk byte[] manipulation.
    /// For RGBA8 images, operates directly on the raw byte array — avoids Color object
    /// allocation overhead per pixel.
    private static Image ApplyColorTransform(Image source, ColorTransform ct)
    {
        Image result = (Image)source.Duplicate();

        if (result.GetFormat() != Image.Format.Rgba8)
        {
            result.Convert(Image.Format.Rgba8);
        }

        byte[] data = result.GetData();
        int length = data.Length;

        // Pre-compute fixed-point multipliers (0-256 range) and offsets (0-255 range)
        // to avoid float math per pixel
        bool hasOffset = Math.Abs(ct.RedOffset) > 0.5f || Math.Abs(ct.GreenOffset) > 0.5f ||
                         Math.Abs(ct.BlueOffset) > 0.5f || Math.Abs(ct.AlphaOffset) > 0.5f;

        if (!hasOffset)
        {
            // Fast path: multipliers only (common case for skin colorization)
            int rMul = (int)(ct.RedMultiplier * 256f);
            int gMul = (int)(ct.GreenMultiplier * 256f);
            int bMul = (int)(ct.BlueMultiplier * 256f);
            int aMul = (int)(ct.AlphaMultiplier * 256f);

            for (int i = 0; i < length; i += 4)
            {
                data[i] = (byte)Math.Clamp((data[i] * rMul) >> 8, 0, 255);
                data[i + 1] = (byte)Math.Clamp((data[i + 1] * gMul) >> 8, 0, 255);
                data[i + 2] = (byte)Math.Clamp((data[i + 2] * bMul) >> 8, 0, 255);
                data[i + 3] = (byte)Math.Clamp((data[i + 3] * aMul) >> 8, 0, 255);
            }
        }
        else
        {
            // Slow path: multipliers + offsets
            float rOff = ct.RedOffset / 255f;
            float gOff = ct.GreenOffset / 255f;
            float bOff = ct.BlueOffset / 255f;
            float aOff = ct.AlphaOffset / 255f;

            for (int i = 0; i < length; i += 4)
            {
                data[i] = (byte)Math.Clamp((int)(((data[i] / 255f * ct.RedMultiplier) + rOff) * 255f), 0, 255);
                data[i + 1] = (byte)Math.Clamp((int)(((data[i + 1] / 255f * ct.GreenMultiplier) + gOff) * 255f), 0, 255);
                data[i + 2] = (byte)Math.Clamp((int)(((data[i + 2] / 255f * ct.BlueMultiplier) + bOff) * 255f), 0, 255);
                data[i + 3] = (byte)Math.Clamp((int)(((data[i + 3] / 255f * ct.AlphaMultiplier) + aOff) * 255f), 0, 255);
            }
        }

        result.SetData(result.GetWidth(), result.GetHeight(), false, Image.Format.Rgba8, data);

        return result;
    }
}
