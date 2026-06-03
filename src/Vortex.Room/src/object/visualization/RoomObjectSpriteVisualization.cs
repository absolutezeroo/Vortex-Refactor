using System;

using Godot;

using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.RoomObjectSpriteVisualization
public class RoomObjectSpriteVisualization : IRoomObjectSpriteVisualization
{
    protected const string LAYER_SEPARATOR = "_";
    protected const string ICON_LAYER_ID = "_icon_";

    private static int s_instanceCounter;

    private List<RoomObjectSprite>? _sprites;
    private IGraphicAssetCollection? _assetCollection;

    protected int _previousScale = -1;
    protected int _previousDirection = -1;
    protected int _previousState = -1;

    public RoomObjectSpriteVisualization()
    {
        InstanceId = s_instanceCounter++;
        _sprites = [];
        Object = null;
        _assetCollection = null;
    }

    public int InstanceId { get; }

    public int UpdateId { get; private set; }

    public int SpriteCount => _sprites?.Count ?? 0;

    public IRoomObject? Object { get; set; }

    public IGraphicAssetCollection? AssetCollection
    {
        get => _assetCollection;
        set
        {
            _assetCollection?.RemoveReference();
            _assetCollection = value;
            _assetCollection?.AddReference();
        }
    }

    public virtual void SetExternalBaseUrls(string baseUrl, string baseUrlSecure, bool useSecure)
    {
    }

    public virtual void Dispose()
    {
        if (_sprites != null)
        {
            while (_sprites.Count > 0)
            {
                RoomObjectSprite sprite = _sprites[0];
                sprite.Dispose();
                _sprites.RemoveAt(_sprites.Count - 1);
            }
            _sprites = null;
        }
        Object = null;
        AssetCollection = null;
    }

    protected void CreateSprites(int count)
    {
        while (_sprites!.Count > count)
        {
            RoomObjectSprite sprite = _sprites[^1];
            sprite.Dispose();
            _sprites.RemoveAt(_sprites.Count - 1);
        }
        while (_sprites.Count < count)
        {
            RoomObjectSprite sprite = new();
            _sprites.Add(sprite);
        }
    }

    public IRoomObjectSprite AddSprite()
    {
        return AddSpriteAt(_sprites!.Count);
    }

    public IRoomObjectSprite AddSpriteAt(int index)
    {
        RoomObjectSprite sprite = new();
        if (index >= _sprites!.Count)
        {
            _sprites.Add(sprite);
        }
        else
        {
            _sprites.Insert(index, sprite);
        }
        return sprite;
    }

    public void RemoveSprite(IRoomObjectSprite sprite)
    {
        int index = _sprites!.IndexOf((RoomObjectSprite)sprite);
        if (index == -1)
        {
            throw new InvalidOperationException("Trying to remove non-existing sprite!");
        }
        _sprites.RemoveAt(index);
        ((RoomObjectSprite)sprite).Dispose();
    }

    public IRoomObjectSprite? GetSprite(int index)
    {
        if (index >= 0 && _sprites != null && index < _sprites.Count)
        {
            return _sprites[index];
        }
        return null;
    }

    public virtual void Update(IRoomGeometry geometry, int time, bool full, bool skip)
    {
    }

    protected void IncreaseUpdateId()
    {
        UpdateId++;
    }

    protected void Reset()
    {
        _previousScale = unchecked((int)0xFFFFFFFF);
        _previousDirection = unchecked((int)0xFFFFFFFF);
        _previousState = -1;
    }

    public virtual List<IRoomObjectSprite>? GetSpriteList()
    {
        return null;
    }

    public virtual bool Initialize(IRoomObjectVisualizationData data)
    {
        return false;
    }

    public Image? GetImage()
    {
        return GetImage(0, -1);
    }

    Image? IRoomObjectVisualization.GetImage(int width, int height)
    {
        return GetImage(width, height);
    }

    private static double NormalizeColorComponent(int value)
    {
        return Math.Max(0, Math.Min(255, value)) / 255.0;
    }

    public Image? GetImage(int bgColor, int unused)
    {
        Rect2I rect = BoundingRectangle;

        if (rect.Size.X * rect.Size.Y == 0)
        {
            return null;
        }

        int count = SpriteCount;
        List<IRoomObjectSprite> visibleSprites = new();

        for (int i = 0; i < count; i++)
        {
            IRoomObjectSprite? sprite = GetSprite(i);

            if (sprite is
                {
                    Visible: true,
                    Asset: not null,
                })
            {
                visibleSprites.Add(sprite);
            }
        }

        visibleSprites.Sort((a, b) => b.RelativeDepth.CompareTo(a.RelativeDepth));

        int width = rect.Size.X;
        int height = rect.Size.Y;

        if (width <= 0 || height <= 0)
        {
            return Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        }

        Image? result = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);

        if (bgColor != 0)
        {
            float r = ((bgColor >> 16) & 0xFF) / 255f;
            float g = ((bgColor >> 8) & 0xFF) / 255f;
            float b = (bgColor & 0xFF) / 255f;
            result.Fill(new Color(r, g, b, 1f));
        }

        foreach (IRoomObjectSprite sprite in visibleSprites)
        {
            Image? asset = sprite.Asset;

            if (asset == null)
            {
                continue;
            }

            Image? srcImage = asset;
            int color = sprite.Color;
            int cr = (color >> 16) & 0xFF;
            int cg = (color >> 8) & 0xFF;
            int cb = color & 0xFF;

            bool hasColorTint = cr < 255 || cg < 255 || cb < 255;
            bool hasAlpha = sprite.Alpha < 255;

            if (bgColor == 0 && sprite.BlendMode == "add")
            {
                srcImage = ExtractDarknessToAlpha(srcImage);
            }

            int srcW = srcImage.GetWidth();
            int srcH = srcImage.GetHeight();

            if (sprite.FlipH || sprite.FlipV)
            {
                srcImage = (Image)srcImage.Duplicate();

                if (sprite.FlipH)
                {
                    srcImage.FlipX();
                }

                if (sprite.FlipV)
                {
                    srcImage.FlipY();
                }
            }

            int destX = sprite.OffsetX - rect.Position.X;
            int destY = sprite.OffsetY - rect.Position.Y;

            if (hasColorTint || hasAlpha)
            {
                double rMul = NormalizeColorComponent(cr);
                double gMul = NormalizeColorComponent(cg);
                double bMul = NormalizeColorComponent(cb);
                double aMul = hasAlpha ? NormalizeColorComponent(sprite.Alpha) : 1.0;

                for (int py = 0; py < srcH; py++)
                {
                    for (int px = 0; px < srcW; px++)
                    {
                        int dx = destX + px;
                        int dy = destY + py;

                        if (dx < 0 || dy < 0 || dx >= width || dy >= height)
                        {
                            continue;
                        }

                        Color srcPixel = srcImage.GetPixel(px, py);

                        if (srcPixel.A <= 0)
                        {
                            continue;
                        }

                        float sr = (float)(srcPixel.R * rMul);
                        float sg = (float)(srcPixel.G * gMul);
                        float sb = (float)(srcPixel.B * bMul);
                        float sa = (float)(srcPixel.A * aMul);
                        Color dstPixel = result.GetPixel(dx, dy);
                        float outA = sa + (dstPixel.A * (1 - sa));

                        if (!(outA > 0))
                        {
                            continue;
                        }

                        float outR = ((sr * sa) + (dstPixel.R * dstPixel.A * (1 - sa))) / outA;
                        float outG = ((sg * sa) + (dstPixel.G * dstPixel.A * (1 - sa))) / outA;
                        float outB = ((sb * sa) + (dstPixel.B * dstPixel.A * (1 - sa))) / outA;

                        result.SetPixel(dx, dy, new Color(outR, outG, outB, outA));
                    }
                }
            }
            else
            {
                result.BlendRect(srcImage, new Rect2I(0, 0, srcW, srcH), new Vector2I(destX, destY));
            }
        }

        return result;
    }

    public Rect2I BoundingRectangle
    {
        get
        {
            int count = SpriteCount;
            int left = 0;
            int top = 0;
            int right = 0;
            int bottom = 0;
            bool first = true;

            for (int i = 0; i < count; i++)
            {
                IRoomObjectSprite? sprite = GetSprite(i);

                if (sprite is not
                    {
                        Visible: true,
                    } || sprite.Asset == null)
                {
                    continue;
                }

                int ox = sprite.OffsetX;
                int oy = sprite.OffsetY;

                if (first)
                {
                    left = ox;
                    top = oy;
                    right = ox + sprite.Width;
                    bottom = oy + sprite.Height;
                    first = false;
                }
                else
                {
                    if (ox < left)
                    {
                        left = ox;
                    }

                    if (oy < top)
                    {
                        top = oy;
                    }

                    if (ox + sprite.Width > right)
                    {
                        right = ox + sprite.Width;
                    }

                    if (oy + sprite.Height > bottom)
                    {
                        bottom = oy + sprite.Height;
                    }
                }
            }

            return new Rect2I(left, top, right - left, bottom - top);
        }
    }

    private static Image ExtractDarknessToAlpha(Image source)
    {
        int width = source.GetWidth();
        int height = source.GetHeight();
        Image? result = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = source.GetPixel(x, y);
                float a = pixel.A;
                int r = (int)(pixel.R * 255);
                int g = (int)(pixel.G * 255);
                int b = (int)(pixel.B * 255);
                int rgb = (r << 16) | (g << 8) | b;
                int hsl = ColorConverter.RgbToHSL(rgb);
                int lightness = hsl & 0xFF;

                if (lightness <= 128)
                {
                    int h = (hsl >> 16) & 0xFF;
                    int s = (hsl >> 8) & 0xFF;
                    a *= lightness / 128f;
                    lightness = 128;
                    hsl = (h << 16) + (s << 8) + lightness;
                    rgb = ColorConverter.HslToRGB(hsl);
                    r = (rgb >> 16) & 0xFF;
                    g = (rgb >> 8) & 0xFF;
                    b = rgb & 0xFF;
                }

                result.SetPixel(x, y, new Color(r / 255f, g / 255f, b / 255f, a));
            }
        }

        return result;
    }
}
