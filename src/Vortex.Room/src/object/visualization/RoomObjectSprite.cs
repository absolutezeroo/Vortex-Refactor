using Godot;

using Vortex.Room.Object.Enum;

namespace Vortex.Room.Object.Visualization;

/// @see com.sulake.room.object.visualization.RoomObjectSprite
public sealed class RoomObjectSprite : IRoomObjectSprite
{
    private static int s_instanceCounter;

    private Image? _asset;
    private string _assetName = "";
    private string _libraryAssetName = "";
    private bool _visible = true;
    private string _tag = "";
    private int _alpha = 255;
    private int _color = 0xFFFFFF;
    private string _blendMode = "normal";
    private bool _flipH;
    private bool _flipV;
    private int _offsetX;
    private int _offsetY;
    private double _relativeDepth;
    private bool _varyingDepth;
    private int _alphaTolerance = 128;
    private bool _clickHandling;
    private object[]? _filters;

    public RoomObjectSprite()
    {
        InstanceId = s_instanceCounter++;
    }

    public void Dispose()
    {
        _asset = null;
        Width = 0;
        Height = 0;
    }

    public Image? Asset
    {
        get => _asset;
        set
        {
            if (value == _asset)
            {
                return;
            }
            if (value != null)
            {
                Width = value.GetWidth();
                Height = value.GetHeight();
            }
            _asset = value;
            UpdateId++;
        }
    }

    public string? AssetName
    {
        get => _assetName;
        set
        {
            if (value == _assetName)
            {
                return;
            }
            _assetName = value ?? "";
            UpdateId++;
        }
    }

    public string? LibraryAssetName
    {
        get => _libraryAssetName;
        set => _libraryAssetName = value ?? "";
    }

    public string? AssetPosture { get; set; }

    public string? AssetGesture { get; set; }

    public bool Visible
    {
        get => _visible;
        set
        {
            if (value == _visible)
            {
                return;
            }
            _visible = value;
            UpdateId++;
        }
    }

    public string? Tag
    {
        get => _tag;
        set
        {
            if (value == _tag)
            {
                return;
            }
            _tag = value ?? "";
            UpdateId++;
        }
    }

    public int Alpha
    {
        get => _alpha;
        set
        {
            value &= 0xFF;
            if (value == _alpha)
            {
                return;
            }
            _alpha = value;
            UpdateId++;
        }
    }

    public int Color
    {
        get => _color;
        set
        {
            value &= 0xFFFFFF;
            if (value == _color)
            {
                return;
            }
            _color = value;
            UpdateId++;
        }
    }

    public string? BlendMode
    {
        get => _blendMode;
        set
        {
            if (value == _blendMode)
            {
                return;
            }
            _blendMode = value ?? "normal";
            UpdateId++;
        }
    }

    public object[]? Filters
    {
        get => _filters;
        set
        {
            if (value == _filters)
            {
                return;
            }
            _filters = value;
            UpdateId++;
        }
    }

    public bool FlipH
    {
        get => _flipH;
        set
        {
            if (value == _flipH)
            {
                return;
            }
            _flipH = value;
            UpdateId++;
        }
    }

    public bool FlipV
    {
        get => _flipV;
        set
        {
            if (value == _flipV)
            {
                return;
            }
            _flipV = value;
            UpdateId++;
        }
    }

    public int Direction { get; set; }

    public int OffsetX
    {
        get => _offsetX;
        set
        {
            if (value == _offsetX)
            {
                return;
            }
            _offsetX = value;
            UpdateId++;
        }
    }

    public int OffsetY
    {
        get => _offsetY;
        set
        {
            if (value == _offsetY)
            {
                return;
            }
            _offsetY = value;
            UpdateId++;
        }
    }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public double RelativeDepth
    {
        get => _relativeDepth;
        set
        {
            if (value == _relativeDepth)
            {
                return;
            }
            _relativeDepth = value;
            UpdateId++;
        }
    }

    public bool VaryingDepth
    {
        get => _varyingDepth;
        set
        {
            if (value == _varyingDepth)
            {
                return;
            }
            _varyingDepth = value;
            UpdateId++;
        }
    }

    public bool ClickHandling
    {
        get => _clickHandling;
        set
        {
            if (_clickHandling == value)
            {
                return;
            }
            _clickHandling = value;
            UpdateId++;
        }
    }

    public bool SkipMouseHandling { get; set; }

    public int InstanceId { get; }

    public int UpdateId { get; private set; }

    public int SpriteType { get; set; } = RoomObjectSpriteType.DEFAULT;

    public string? ObjectType { get; set; }

    public int AlphaTolerance
    {
        get => _alphaTolerance;
        set
        {
            if (_alphaTolerance == value)
            {
                return;
            }
            _alphaTolerance = value;
            UpdateId++;
        }
    }

    public int PlaneId { get; set; }
}
