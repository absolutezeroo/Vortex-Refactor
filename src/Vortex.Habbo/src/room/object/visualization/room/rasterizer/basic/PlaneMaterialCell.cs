using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Habbo.Room.Object.Visualization.Room.Utils;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneMaterialCell
public class PlaneMaterialCell
{
    private Image? _cachedBitmap;
    private PlaneTexture? _texture;
    private readonly List<Vector2I> _offsets = [];
    private readonly List<IGraphicAsset> _extraItems = [];
    private readonly int _extraItemCount;

    public PlaneMaterialCell(PlaneTexture? texture, List<IGraphicAsset>? extraItems = null, List<Vector2I>? offsets = null,
        int extraItemCount = 0)
    {
        _texture = texture;
        if (extraItems is
                not
                {
                    Count: > 0,
                } || extraItemCount <= 0)
        {
            return;
        }

        foreach (IGraphicAsset item in extraItems)
        {
            _extraItems.Add(item);
        }

        if (_extraItems.Count <= 0)
        {
            return;
        }

        if (offsets != null)
        {
            foreach (Vector2I offset in offsets)
            {
                _offsets.Add(new Vector2I(offset.X, offset.Y));
            }
        }

        _extraItemCount = extraItemCount;
    }

    public bool IsStatic => _extraItemCount == 0;

    public void Dispose()
    {
        if (_texture != null)
        {
            _texture.Dispose();
            _texture = null;
        }

        _cachedBitmap = null;
    }

    public void ClearCache()
    {
        _cachedBitmap = null;
    }

    public int GetHeight(IVector3d? normal)
    {
        if (_texture == null)
        {
            return 0;
        }

        Image? bitmap = _texture.GetBitmap(normal);

        if (bitmap != null)
        {
            return bitmap.GetHeight();
        }

        return 0;
    }

    public Image? Render(IVector3d? normal, int offsetX, int offsetY)
    {
        if (_texture == null)
        {
            return null;
        }

        Image? texBitmap = _texture.GetBitmap(normal);

        if (texBitmap == null)
        {
            return null;
        }

        Image? result = texBitmap;

        // Apply tiling offset
        if (offsetX != 0 || offsetY != 0)
        {
            int w = texBitmap.GetWidth();
            int h = texBitmap.GetHeight();

            Image? doubled = Image.CreateEmpty(w * 2, h * 2, false, Image.Format.Rgba8);
            doubled.BlitRect(texBitmap, new Rect2I(0, 0, w, h), Vector2I.Zero);
            doubled.BlitRect(texBitmap, new Rect2I(0, 0, w, h), new Vector2I(w, 0));
            doubled.BlitRect(texBitmap, new Rect2I(0, 0, w, h), new Vector2I(0, h));
            doubled.BlitRect(texBitmap, new Rect2I(0, 0, w, h), new Vector2I(w, h));

            result = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);

            while (offsetX < 0)
            {
                offsetX += w;
            }

            while (offsetY < 0)
            {
                offsetY += h;
            }

            result.BlitRect(doubled, new Rect2I(offsetX % w, offsetY % h, w, h), Vector2I.Zero);
        }

        if (!IsStatic)
        {
            int w = result.GetWidth();
            int h = result.GetHeight();

            if (_cachedBitmap != null)
            {
                if (_cachedBitmap.GetWidth() != w || _cachedBitmap.GetHeight() != h)
                {
                    _cachedBitmap = null;
                }
                else
                {
                    _cachedBitmap.BlitRect(result, new Rect2I(0, 0, w, h), Vector2I.Zero);
                }
            }

            _cachedBitmap ??= (Image)result.Duplicate();

            int count = Math.Min(_extraItemCount, _offsets.Count);
            int max = Math.Max(_extraItemCount, _offsets.Count);
            int[]? indices = Randomizer.GetArray(_extraItemCount, max);
            if (indices == null)
            {
                return _cachedBitmap;
            }

            for (int i = 0; i < count; i++)
            {
                Vector2I offset = _offsets[indices[i]];
                IGraphicAsset asset = _extraItems[i % _extraItems.Count];

                if (asset.Asset is not BitmapDataAsset
                    {
                        Content: Image assetBitmap,
                    })
                {
                    continue;
                }

                int drawX = offset.X + asset.OffsetX;
                int drawY = offset.Y + asset.OffsetY;

                // Handle flipping
                Image? drawBitmap = assetBitmap;

                if (asset.FlipH || asset.FlipV)
                {
                    drawBitmap = (Image)assetBitmap.Duplicate();

                    if (asset.FlipH)
                    {
                        drawBitmap.FlipX();
                    }

                    if (asset.FlipV)
                    {
                        drawBitmap.FlipY();
                    }

                    if (asset.FlipH)
                    {
                        drawX += assetBitmap.GetWidth();
                    }
                    if (asset.FlipV)
                    {
                        drawY += assetBitmap.GetHeight();
                    }
                }

                // Align X to even
                drawX = (drawX >> 1) << 1;

                BlitWithAlpha(_cachedBitmap, drawBitmap,
                    new Rect2I(0, 0, drawBitmap.GetWidth(), drawBitmap.GetHeight()),
                    new Vector2I(drawX, drawY));
            }

            return _cachedBitmap;
        }

        return result;
    }

    public string? GetAssetName(IVector3d? normal)
    {
        return _texture?.GetAssetName(normal);
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
                            float outA = srcColor.A + (dstColor.A * (1f - srcColor.A));

                            if (outA > 0f)
                            {
                                float r = ((srcColor.R * srcColor.A) + (dstColor.R * dstColor.A * (1f - srcColor.A))) / outA;
                                float g = ((srcColor.G * srcColor.A) + (dstColor.G * dstColor.A * (1f - srcColor.A))) / outA;
                                float b = ((srcColor.B * srcColor.A) + (dstColor.B * dstColor.A * (1f - srcColor.A))) / outA;

                                dest.SetPixel(dx, dy, new Color(r, g, b, outA));
                            }

                            break;
                        }
                }

            }
        }
    }
}
