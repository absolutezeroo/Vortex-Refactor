using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Animated;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.animated.PlaneVisualizationAnimationLayer
public class PlaneVisualizationAnimationLayer : IDisposable
{
    private Image? _bitmapData;
    private List<AnimationItem> _items = [];

    public PlaneVisualizationAnimationLayer(XElement? xml, IGraphicAssetCollection? assetCollection)
    {
        if (xml == null || assetCollection == null)
        {
            return;
        }

        foreach (XElement itemElement in xml.Elements("item"))
        {
            string? assetName = (string?)itemElement.Attribute("asset");

            if (string.IsNullOrEmpty(assetName))
            {
                continue;
            }

            IGraphicAsset? graphicAsset = assetCollection.GetAsset(assetName);

            if (graphicAsset == null)
            {
                continue;
            }

            if (graphicAsset.Asset is not BitmapDataAsset { Content: Image bitmapData })
            {
                continue;
            }

            double x = (double?)itemElement.Attribute("x") ?? 0;
            double y = (double?)itemElement.Attribute("y") ?? 0;
            double speedX = (double?)itemElement.Attribute("speedX") ?? 0;
            double speedY = (double?)itemElement.Attribute("speedY") ?? 0;

            AnimationItem item = new(x, y, speedX, speedY, bitmapData);
            _items.Add(item);
        }
    }

    public bool Disposed { get; private set; }

    bool Core.Runtime.IDisposable.disposed => Disposed;

    public void Dispose()
    {
        Disposed = true;

        if (_bitmapData != null)
        {
            _bitmapData = null;
        }

        foreach (AnimationItem item in _items)
        {
            item.Dispose();
        }

        _items = [];
    }

    public void ClearCache()
    {
        _bitmapData = null;
    }

    /// @see com.sulake.habbo.room.object.visualization.room.rasterizer.animated.PlaneVisualizationAnimationLayer#render
    public Image? Render(
        Image? canvas, int width, int height, IVector3d normal,
        int offsetX, int offsetY,
        int tileWidth, int tileHeight,
        double scrollX, double scrollY, int timeStamp)
    {
        if (canvas == null || canvas.GetWidth() != width || canvas.GetHeight() != height)
        {
            if (_bitmapData == null || _bitmapData.GetWidth() != width || _bitmapData.GetHeight() != height)
            {
                _bitmapData = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
                _bitmapData.Fill(Colors.White);
            }
            else
            {
                _bitmapData.Fill(Colors.White);
            }

            canvas = _bitmapData;
        }

        if (tileWidth <= 0 || tileHeight <= 0)
        {
            return canvas;
        }

        foreach (AnimationItem item in _items)
        {
            Vector2I pos = item.GetPosition(tileWidth, tileHeight, scrollX, scrollY, timeStamp);
            pos -= new Vector2I(offsetX, offsetY);

            if (item.BitmapData == null)
            {
                continue;
            }

            int bw = item.BitmapData.GetWidth();
            int bh = item.BitmapData.GetHeight();
            Rect2I srcRect = new(0, 0, bw, bh);

            // Original position
            if (pos.X > -bw && pos.X < width && pos.Y > -bh && pos.Y < height)
            {
                BlitWithAlpha(canvas, item.BitmapData, srcRect, pos);
            }

            // Horizontal wrap
            if (pos.X - tileWidth > -bw && pos.X - tileWidth < width && pos.Y > -bh && pos.Y < height)
            {
                BlitWithAlpha(canvas, item.BitmapData, srcRect, new Vector2I(pos.X - tileWidth, pos.Y));
            }

            // Vertical wrap
            if (pos.X > -bw && pos.X < width && pos.Y - tileHeight > -bh && pos.Y - tileHeight < height)
            {
                BlitWithAlpha(canvas, item.BitmapData, srcRect, new Vector2I(pos.X, pos.Y - tileHeight));
            }

            // Diagonal wrap
            if (pos.X - tileWidth > -bw && pos.X - tileWidth < width &&
                pos.Y - tileHeight > -bh && pos.Y - tileHeight < height)
            {
                BlitWithAlpha(canvas, item.BitmapData, srcRect,
                    new Vector2I(pos.X - tileWidth, pos.Y - tileHeight));
            }
        }

        return canvas;
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
                                float r = (srcColor.R * srcColor.A + dstColor.R * dstColor.A * (1f - srcColor.A)) / outA;
                                float g = (srcColor.G * srcColor.A + dstColor.G * dstColor.A * (1f - srcColor.A)) / outA;
                                float b = (srcColor.B * srcColor.A + dstColor.B * dstColor.A * (1f - srcColor.A)) / outA;

                                dest.SetPixel(dx, dy, new Color(r, g, b, outA));
                            }

                            break;
                        }
                }
            }
        }
    }
}
