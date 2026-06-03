using System;
using System.Text;

using Godot;

using Vortex.Room.Object;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Furniture;

/// @see com.sulake.habbo.room.object.visualization.furniture.FurnitureVisualization
public class FurnitureVisualization : RoomObjectSpriteVisualization
{
    protected static readonly double Z_MULTIPLIER = Math.Sqrt(0.5);

    private const int UPDATE_INTERVAL_MS = 41;

    private static readonly StringBuilder s_stringBuffer = new();
    private static readonly string?[] s_nameTemplate = new string?[8];

    static FurnitureVisualization()
    {
        s_nameTemplate[1] = "_";
        s_nameTemplate[3] = "_";
        s_nameTemplate[5] = "_";
        s_nameTemplate[7] = "_";
    }

    private int _lastUpdateTime = -1000;
    private double _cachedGeometryDirection = double.NaN;
    private int _selectedColor = -1;
    private string? _adClickUrl;
    private bool _clickHandling;
    private string?[] _cachedAssetNames = [];
    private bool[] _cachedAssetNamesHaveFrame = [];
    private string?[] _cachedTags = [];
    private int?[] _cachedAlphas = [];
    private int?[] _cachedColors = [];
    private int?[] _cachedXOffsets = [];
    private int?[] _cachedYOffsets = [];
    private double?[] _cachedZOffsets = [];
    private bool?[] _cachedMouseCapture = [];
    private int?[] _cachedInk = [];
    private int _updatedLayers;
    private double _liftAmount;
    private bool _lookThrough;
    private bool _lookThroughChanged;
    private bool _filtersChanged;
    private int _cachedDirection = -1;
    private double _cachedScale;
    private int _cachedSize = -1;

    protected double _alphaMultiplier = 1;
    protected bool _alphaChanged = true;
    protected int _totalSpriteCount;
    protected int _shadowSpriteIndex = -1;

    private object[]? _filters;

    public object[]? Filters
    {
        get => _filters;
        set
        {
            _filters = value;
            _filtersChanged = true;
        }
    }

    public bool LookThrough
    {
        set
        {
            if (_lookThrough != value)
            {
                _lookThroughChanged = true;
                _lookThrough = value;
            }
        }
    }

    protected int Direction { get; set; }

    protected string Type { get; private set; } = "";

    protected FurnitureVisualizationData? Data { get; private set; }

    public override void Dispose()
    {
        base.Dispose();
        Data = null;
        _cachedAssetNames = null!;
        _cachedAssetNamesHaveFrame = null!;
        _cachedTags = null!;
        _cachedAlphas = null!;
        _cachedColors = null!;
        _cachedXOffsets = null!;
        _cachedYOffsets = null!;
        _cachedZOffsets = null!;
        _cachedMouseCapture = null!;
        _cachedInk = null!;
        _filters = null;
    }

    public override bool Initialize(IRoomObjectVisualizationData data)
    {
        Reset();

        if (data is not FurnitureVisualizationData furnitureData)
        {
            return false;
        }

        Data = furnitureData;
        Type = Data.GetType();
        return true;
    }

    public override void Update(IRoomGeometry geometry, int time, bool full, bool skip)
    {
        if (geometry == null)
        {
            return;
        }

        if (time < _lastUpdateTime + UPDATE_INTERVAL_MS)
        {
            return;
        }

        _lastUpdateTime += UPDATE_INTERVAL_MS;
        if (_lastUpdateTime + UPDATE_INTERVAL_MS < time)
        {
            _lastUpdateTime = time - UPDATE_INTERVAL_MS;
        }

        bool objectChanged = false;
        double scale = geometry.Scale;

        if (UpdateObject(scale, geometry.Direction.X))
        {
            objectChanged = true;
        }

        if (UpdateModel(scale))
        {
            objectChanged = true;
        }

        if (_lookThroughChanged)
        {
            objectChanged = true;
            _lookThroughChanged = false;
        }

        if (_filtersChanged)
        {
            objectChanged = true;
            _filtersChanged = false;
        }

        int layerMask;
        if (skip)
        {
            _updatedLayers |= UpdateAnimation(scale);
            layerMask = 0;
        }
        else
        {
            layerMask = UpdateAnimation(scale) | _updatedLayers;
            _updatedLayers = 0;
        }

        if (objectChanged || layerMask != 0)
        {
            UpdateSprites((int)scale, objectChanged, layerMask);
            _previousScale = (int)scale;
            IncreaseUpdateId();
        }
    }

    protected virtual void UpdateSprites(int scale, bool fullUpdate, int layerMask)
    {
        if (_totalSpriteCount != SpriteCount)
        {
            CreateSprites(_totalSpriteCount);
        }

        if (fullUpdate)
        {
            for (int i = SpriteCount - 1; i >= 0; i--)
            {
                UpdateSprite(scale, i);
            }
        }
        else
        {
            int layer = 0;
            while (layerMask > 0)
            {
                if ((layerMask & 1) != 0)
                {
                    UpdateSprite(scale, layer);
                }
                layer++;
                layerMask >>= 1;
            }
        }

        _alphaChanged = false;
    }

    protected virtual void UpdateSprite(int scale, int spriteIndex)
    {
        string? assetName = GetSpriteAssetName(scale, spriteIndex);
        IRoomObjectSprite? sprite = GetSprite(spriteIndex);

        if (sprite == null)
        {
            return;
        }

        if (assetName == null)
        {
            ResetSprite(sprite, spriteIndex);
            return;
        }

        IGraphicAsset? asset = GetAsset(assetName, spriteIndex);

        if (asset?.Asset != null)
        {
            sprite.Visible = true;
            sprite.ObjectType = Type;
            sprite.Asset = asset.Asset.Content as Image;

            if (asset.Asset.Content == null)
            {
                _previousState++;
            }

            sprite.FlipH = asset.FlipH;
            sprite.FlipV = asset.FlipV;
            sprite.Direction = Direction;

            double zOffset;

            if (spriteIndex != _shadowSpriteIndex)
            {
                sprite.Tag = GetSpriteTag(scale, Direction, spriteIndex);
                sprite.Alpha = GetSpriteAlpha(scale, Direction, spriteIndex);
                sprite.Color = (int)GetSpriteColor(scale, spriteIndex, _selectedColor);
                sprite.OffsetX = asset.OffsetX + GetSpriteXOffset(scale, Direction, spriteIndex);
                sprite.OffsetY = asset.OffsetY + GetSpriteYOffset(scale, Direction, spriteIndex);
                sprite.AlphaTolerance = GetSpriteMouseCapture(scale, Direction, spriteIndex) ? 128 : 256;
                sprite.BlendMode = GetBlendMode(GetSpriteInk(scale, Direction, spriteIndex));
                zOffset = GetSpriteZOffset(scale, Direction, spriteIndex);
                zOffset -= spriteIndex * 0.001;
            }
            else
            {
                sprite.OffsetX = asset.OffsetX;
                sprite.OffsetY = asset.OffsetY + GetSpriteYOffset(scale, Direction, spriteIndex);
                int alpha = (int)(48 * _alphaMultiplier);
                sprite.Alpha = alpha;
                sprite.AlphaTolerance = 256;
                zOffset = 1;
            }

            if (_lookThrough)
            {
                sprite.Alpha = (int)(sprite.Alpha * 0.2);
            }

            zOffset *= Z_MULTIPLIER;
            sprite.RelativeDepth = zOffset;
            sprite.AssetName = asset.AssetName;
            sprite.LibraryAssetName = GetLibraryAssetNameForSprite(asset, sprite);
            sprite.AssetPosture = GetPostureForAssetFile(scale, asset.LibraryAssetName);
            sprite.ClickHandling = _clickHandling;

            if (sprite.BlendMode != "add")
            {
                sprite.Filters = _filters;
            }
        }
        else
        {
            ResetSprite(sprite, spriteIndex);
        }
    }

    protected virtual string? GetLibraryAssetNameForSprite(IGraphicAsset asset, IRoomObjectSprite sprite)
    {
        return asset.LibraryAssetName;
    }

    protected string GetBlendMode(int ink)
    {
        return ink switch
        {
            1 => "add",
            2 => "subtract",
            3 => "darken",
            4 => "difference",
            5 => "multiply",
            6 => "invert",
            7 => "screen",
            _ => "normal",
        };
    }

    protected virtual bool UpdateObject(double scale, double geometryDirection)
    {
        IRoomObject? obj = Object;
        if (obj == null)
        {
            return false;
        }

        if (_previousState != obj.UpdateId || scale != _previousScale || geometryDirection != _cachedGeometryDirection)
        {
            double angle = ((obj.Direction.X - (geometryDirection + 135)) % 360 + 360) % 360;

            if (Data != null)
            {
                Direction = Data.GetDirectionValue((int)scale, (int)angle);
            }

            _previousState = obj.UpdateId;
            _cachedGeometryDirection = geometryDirection;
            _previousScale = (int)scale;
            UpdateAssetAndSpriteCache(scale, Direction);
            return true;
        }

        return false;
    }

    protected virtual bool UpdateModel(double scale)
    {
        IRoomObject? obj = Object;
        if (obj == null)
        {
            return false;
        }

        IRoomObjectModel model = obj.Model;

        if (_previousDirection != model.UpdateId)
        {
            _selectedColor = (int)model.GetNumber("furniture_color");
            double alphaMultiplier = model.GetNumber("furniture_alpha_multiplier");

            if (double.IsNaN(alphaMultiplier))
            {
                alphaMultiplier = 1;
            }

            if (alphaMultiplier != _alphaMultiplier)
            {
                _alphaMultiplier = alphaMultiplier;
                _alphaChanged = true;
            }

            _adClickUrl = GetAdClickUrl(model);
            _clickHandling = !string.IsNullOrEmpty(_adClickUrl) && _adClickUrl.StartsWith("http");
            _liftAmount = model.GetNumber("furniture_lift_amount");
            _previousDirection = model.UpdateId;
            return true;
        }

        return false;
    }

    protected virtual string? GetAdClickUrl(IRoomObjectModel model)
    {
        return model.GetString("furniture_ad_url");
    }

    protected virtual int UpdateAnimation(double scale)
    {
        return 0;
    }

    protected virtual void UpdateLayerCount(int count)
    {
        _totalSpriteCount = count;
        _shadowSpriteIndex = _totalSpriteCount - GetAdditionalSpriteCount();
    }

    protected virtual int GetAdditionalSpriteCount()
    {
        return 1;
    }

    protected virtual int GetFrameNumber(int scale, int layer)
    {
        return 0;
    }

    protected virtual string? GetPostureForAssetFile(int scale, string? libraryAssetName)
    {
        return null;
    }

    protected virtual IGraphicAsset? GetAsset(string name, int layer = -1)
    {
        return AssetCollection?.GetAsset(name);
    }

    protected virtual string? GetSpriteAssetName(int scale, int spriteIndex)
    {
        if (Data == null || spriteIndex >= FurnitureVisualizationData.LAYER_NAMES.Length)
        {
            return "";
        }

        string? cached = spriteIndex < _cachedAssetNames.Length ? _cachedAssetNames[spriteIndex] : null;
        bool hasFrame = spriteIndex < _cachedAssetNamesHaveFrame.Length && _cachedAssetNamesHaveFrame[spriteIndex];

        if (string.IsNullOrEmpty(cached))
        {
            cached = GetSpriteAssetNameWithoutFrame(scale, spriteIndex, true);
            hasFrame = _cachedSize != 1;
        }

        if (hasFrame)
        {
            cached += GetFrameNumber(scale, spriteIndex);
        }

        return cached;
    }

    protected virtual string GetSpriteAssetNameWithoutFrame(int scale, int spriteIndex, bool useCache)
    {
        int size = useCache ? _cachedSize : GetSize(scale);
        bool isIcon = size == 1;

        string layerCode;

        if (spriteIndex != _shadowSpriteIndex)
        {
            layerCode = FurnitureVisualizationData.LAYER_NAMES[spriteIndex];
        }
        else
        {
            layerCode = "sd";
        }

        if (isIcon)
        {
            return Type + "_icon_" + layerCode;
        }

        s_nameTemplate[0] = Type;
        s_nameTemplate[2] = size.ToString();
        s_nameTemplate[4] = layerCode;
        s_nameTemplate[6] = Direction.ToString();

        s_stringBuffer.Clear();
        foreach (string? part in s_nameTemplate)
        {
            if (part != null)
            {
                s_stringBuffer.Append(part);
            }
        }

        string result = s_stringBuffer.ToString();

        if (!useCache)
        {
            return result;
        }

        EnsureCacheSize(spriteIndex);

        _cachedAssetNames[spriteIndex] = result;
        _cachedAssetNamesHaveFrame[spriteIndex] = !isIcon;

        return result;
    }

    protected virtual string GetSpriteTag(int scale, int direction, int layer)
    {
        if (layer < _cachedTags.Length && _cachedTags[layer] != null)
        {
            return _cachedTags[layer]!;
        }

        if (Data == null)
        {
            return "";
        }

        string tag = Data.GetTag(scale, direction, layer);

        EnsureCacheSize(layer);

        _cachedTags[layer] = tag;

        return tag;
    }

    protected virtual int GetSpriteAlpha(int scale, int direction, int layer)
    {
        if (layer < _cachedAlphas.Length && _cachedAlphas[layer] != null && !_alphaChanged)
        {
            return _cachedAlphas[layer]!.Value;
        }

        if (Data == null)
        {
            return 255;
        }

        int alpha = (int)(Data.GetAlpha(scale, direction, layer) * _alphaMultiplier);

        EnsureCacheSize(layer);

        _cachedAlphas[layer] = alpha;

        return alpha;
    }

    protected virtual uint GetSpriteColor(int scale, int layer, int colorId)
    {
        if (layer < _cachedColors.Length && _cachedColors[layer] != null)
        {
            return (uint)_cachedColors[layer]!.Value;
        }

        if (Data == null)
        {
            return 0xFFFFFF;
        }

        uint color = Data.GetColor(scale, layer, colorId);

        EnsureCacheSize(layer);

        _cachedColors[layer] = (int)color;

        return color;
    }

    protected virtual int GetSpriteXOffset(int scale, int direction, int layer)
    {
        if (layer < _cachedXOffsets.Length && _cachedXOffsets[layer] != null)
        {
            return _cachedXOffsets[layer]!.Value;
        }

        if (Data == null)
        {
            return 0;
        }

        int offset = Data.GetXOffset(scale, direction, layer);
        EnsureCacheSize(layer);
        _cachedXOffsets[layer] = offset;
        return offset;
    }

    protected virtual int GetSpriteYOffset(int scale, int direction, int layer)
    {
        if (layer == _shadowSpriteIndex)
        {
            return (int)Math.Ceiling(_liftAmount * (scale / 2.0));
        }

        if (layer < _cachedYOffsets.Length && _cachedYOffsets[layer] != null)
        {
            return _cachedYOffsets[layer]!.Value;
        }

        if (Data == null)
        {
            return 0;
        }

        int offset = Data.GetYOffset(scale, direction, layer);

        EnsureCacheSize(layer);

        _cachedYOffsets[layer] = offset;

        return offset;

    }

    protected virtual bool GetSpriteMouseCapture(int scale, int direction, int layer)
    {
        if (layer < _cachedMouseCapture.Length && _cachedMouseCapture[layer] != null)
        {
            return _cachedMouseCapture[layer]!.Value;
        }

        if (Data == null)
        {
            return true;
        }

        bool capture = !Data.GetIgnoreMouse(scale, direction, layer);

        EnsureCacheSize(layer);

        _cachedMouseCapture[layer] = capture;

        return capture;
    }

    protected virtual int GetSpriteInk(int scale, int direction, int layer)
    {
        if (layer < _cachedInk.Length && _cachedInk[layer] != null)
        {
            return _cachedInk[layer]!.Value;
        }

        if (Data == null)
        {
            return 0;
        }

        int ink = Data.GetInk(scale, direction, layer);

        EnsureCacheSize(layer);

        _cachedInk[layer] = ink;

        return ink;
    }

    protected virtual double GetSpriteZOffset(int scale, int direction, int layer)
    {
        if (layer < _cachedZOffsets.Length && _cachedZOffsets[layer] != null)
        {
            return _cachedZOffsets[layer]!.Value;
        }

        if (Data == null)
        {
            return 0;
        }

        double offset = Data.GetZOffset(scale, direction, layer);

        EnsureCacheSize(layer);

        _cachedZOffsets[layer] = offset;

        return offset;
    }

    protected int GetSize(int scale)
    {
        if (Data != null)
        {
            return Data.GetSize(scale);
        }

        return scale;
    }

    private void ResetSprite(IRoomObjectSprite sprite, int spriteIndex)
    {
        sprite.Asset = null;
        sprite.AssetName = "";
        sprite.AssetPosture = null;
        sprite.Alpha = 0;
        sprite.Tag = "";
        sprite.FlipH = false;
        sprite.FlipV = false;
        sprite.OffsetX = 0;
        sprite.OffsetY = 0;
        sprite.RelativeDepth = 0;
        sprite.ClickHandling = false;

        if (_alphaChanged && spriteIndex < _cachedAlphas.Length)
        {
            _cachedAlphas[spriteIndex] = null;
        }
    }

    private void UpdateAssetAndSpriteCache(double scale, int direction)
    {
        if (_cachedDirection == direction && _cachedScale == scale)
        {
            return;
        }

        _cachedAssetNames = [];
        _cachedAssetNamesHaveFrame = [];
        _cachedTags = [];
        _cachedAlphas = [];
        _cachedColors = [];
        _cachedXOffsets = [];
        _cachedYOffsets = [];
        _cachedZOffsets = [];
        _cachedMouseCapture = [];
        _cachedInk = [];
        _cachedDirection = direction;
        _cachedScale = scale;
        _cachedSize = GetSize((int)scale);

        UpdateLayerCount(Data!.GetLayerCount((int)scale) + GetAdditionalSpriteCount());
    }

    private void EnsureCacheSize(int index)
    {
        int required = index + 1;

        if (_cachedAssetNames.Length >= required)
        {
            return;
        }

        Array.Resize(ref _cachedAssetNames, required);
        Array.Resize(ref _cachedAssetNamesHaveFrame, required);
        Array.Resize(ref _cachedTags, required);
        Array.Resize(ref _cachedAlphas, required);
        Array.Resize(ref _cachedColors, required);
        Array.Resize(ref _cachedXOffsets, required);
        Array.Resize(ref _cachedYOffsets, required);
        Array.Resize(ref _cachedZOffsets, required);
        Array.Resize(ref _cachedMouseCapture, required);
        Array.Resize(ref _cachedInk, required);
    }
}
