using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Mask;

/// @see com.sulake.habbo.room.object.visualization.room.mask.PlaneMaskManager
public class PlaneMaskManager
{
    private IGraphicAssetCollection? _assetCollection;
    private Dictionary<string, PlaneMask>? _masks = [];

    public XElement? Data { get; private set; }

    public void Dispose()
    {
        _assetCollection = null;
        Data = null;

        if (_masks == null)
        {
            return;
        }

        foreach (PlaneMask mask in _masks.Values)
        {
            mask.Dispose();
        }

        _masks = null;
    }

    public void Initialize(XElement? data)
    {
        Data = data;
    }

    public void InitializeAssetCollection(IGraphicAssetCollection? assetCollection)
    {
        if (Data == null)
        {
            return;
        }

        _assetCollection = assetCollection;
        ParseMasks(Data, assetCollection);
    }

    public bool UpdateMask(Image? target, string type, double scale, IVector3d? normal, int posX, int posY)
    {
        if (_masks == null || target == null)
        {
            return true;
        }

        _masks.TryGetValue(type, out PlaneMask? mask);

        if (mask == null)
        {
            return true;
        }

        IGraphicAsset? asset = mask.GetGraphicAsset(scale, normal!);

        if (asset == null)
        {
            return true;
        }

        if (asset.Asset is not BitmapDataAsset { Content: Image bitmapData })
        {
            return true;
        }

        int drawX = posX + asset.OffsetX;
        int drawY = posY + asset.OffsetY;

        int scaleX = 1;
        int scaleY = 1;
        int flipOffsetX = 0;
        int flipOffsetY = 0;

        if (asset.FlipH)
        {
            scaleX = -1;
            flipOffsetX = bitmapData.GetWidth();
        }

        if (asset.FlipV)
        {
            scaleY = -1;
            flipOffsetY = bitmapData.GetHeight();
        }

        Image drawImage = bitmapData;

        if (scaleX < 0 || scaleY < 0)
        {
            drawImage = (Image)bitmapData.Duplicate();

            if (scaleX < 0)
            {
                drawImage.FlipX();
            }

            if (scaleY < 0)
            {
                drawImage.FlipY();
            }
        }

        int destX = drawX + flipOffsetX;
        int destY = drawY + flipOffsetY;

        // Blit the mask image onto the target
        int sw = drawImage.GetWidth();
        int sh = drawImage.GetHeight();
        int dw = target.GetWidth();
        int dh = target.GetHeight();

        for (int y = 0; y < sh; y++)
        {
            int dy = destY + y;

            if (dy < 0 || dy >= dh)
            {
                continue;
            }

            for (int x = 0; x < sw; x++)
            {
                int dx = destX + x;

                if (dx < 0 || dx >= dw)
                {
                    continue;
                }

                Color srcColor = drawImage.GetPixel(x, y);
                Color dstColor = target.GetPixel(dx, dy);

                // Darken blend: min of each channel
                float r = Math.Min(srcColor.R, dstColor.R);
                float g = Math.Min(srcColor.G, dstColor.G);
                float b = Math.Min(srcColor.B, dstColor.B);
                float a = Math.Max(srcColor.A, dstColor.A);

                target.SetPixel(dx, dy, new Color(r, g, b, a));
            }
        }

        return true;
    }

    public PlaneMask? GetMask(string type)
    {
        if (_masks == null)
        {
            return null;
        }

        _masks.TryGetValue(type, out PlaneMask? mask);
        return mask;
    }

    private void ParseMasks(XElement data, IGraphicAssetCollection? assetCollection)
    {
        if (assetCollection == null || _masks == null)
        {
            return;
        }

        foreach (XElement maskElement in data.Elements("mask"))
        {
            string? id = (string?)maskElement.Attribute("id");

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            if (_masks.ContainsKey(id))
            {
                continue;
            }

            PlaneMask mask = new();

            foreach (XElement vizElement in maskElement.Elements("maskVisualization"))
            {
                XAttribute? sizeAttr = vizElement.Attribute("size");

                if (sizeAttr == null)
                {
                    continue;
                }

                int size = (int)sizeAttr;

                PlaneMaskVisualization? viz = mask.CreateMaskVisualization(size);

                if (viz == null)
                {
                    continue;
                }

                string? assetName = ParseMaskBitmaps(vizElement.Elements("bitmap"), viz, assetCollection);
                mask.SetAssetName(size, assetName);
            }

            _masks[id] = mask;
        }
    }

    private static string? ParseMaskBitmaps(IEnumerable<XElement> bitmapElements,
        PlaneMaskVisualization viz, IGraphicAssetCollection assetCollection)
    {
        string? result = null;

        foreach (XElement bitmapElement in bitmapElements)
        {
            string? assetName = (string?)bitmapElement.Attribute("assetName");

            if (string.IsNullOrEmpty(assetName))
            {
                continue;
            }

            double normalMinX = (double?)bitmapElement.Attribute("normalMinX") ?? -1;
            double normalMaxX = (double?)bitmapElement.Attribute("normalMaxX") ?? 1;
            double normalMinY = (double?)bitmapElement.Attribute("normalMinY") ?? -1;
            double normalMaxY = (double?)bitmapElement.Attribute("normalMaxY") ?? 1;

            IGraphicAsset? asset = assetCollection.GetAsset(assetName);

            if (asset == null)
            {
                continue;
            }

            if (!asset.FlipH)
            {
                result = assetName;
            }

            viz.AddBitmap(asset, normalMinX, normalMaxX, normalMinY, normalMaxY);
        }

        return result;
    }
}
