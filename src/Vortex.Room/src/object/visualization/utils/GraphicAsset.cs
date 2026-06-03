using Godot;

using Vortex.Core.Assets;

namespace Vortex.Room.Object.Visualization.Utils;

/// @see com.sulake.room.object.visualization.utils.GraphicAsset
public class GraphicAsset : IGraphicAsset
{
    private static readonly List<GraphicAsset> s_pool = [];

    private BitmapDataAsset? _asset;
    private int _width;
    private int _height;
    private bool _initialized;

    public static GraphicAsset Allocate(
        string name, string libraryName, IAsset? asset,
        bool flipH, bool flipV, int offsetX, int offsetY,
        bool usesPalette = false)
    {
        GraphicAsset instance;
        if (s_pool.Count > 0)
        {
            instance = s_pool[^1];
            s_pool.RemoveAt(s_pool.Count - 1);
        }
        else
        {
            instance = new GraphicAsset();
        }

        instance.AssetName = name;
        instance.LibraryAssetName = libraryName;

        if (asset is BitmapDataAsset bitmapAsset)
        {
            instance._asset = bitmapAsset;
            instance._initialized = false;
        }
        else
        {
            instance._asset = null;
            instance._initialized = true;
        }

        instance.FlipH = flipH;
        instance.FlipV = flipV;
        instance.OriginalOffsetX = offsetX;
        instance.OriginalOffsetY = offsetY;
        instance.UsesPalette = usesPalette;
        instance._width = 0;
        instance._height = 0;
        return instance;
    }

    public void Recycle()
    {
        _asset = null;
        s_pool.Add(this);
    }

    private void Initialize()
    {
        if (_initialized || _asset == null)
        {
            return;
        }
        if (_asset.Content is Image image)
        {
            _width = image.GetWidth();
            _height = image.GetHeight();
        }
        _initialized = true;
    }

    public bool FlipV { get; private set; }

    public bool FlipH { get; private set; }

    public int Width
    {
        get
        {
            Initialize();
            return _width;
        }
    }

    public int Height
    {
        get
        {
            Initialize();
            return _height;
        }
    }

    public string? AssetName { get; private set; }

    public string? LibraryAssetName { get; private set; }

    public IAsset? Asset => _asset;
    public bool UsesPalette { get; private set; }

    public int OffsetX
    {
        get
        {
            if (!FlipH)
            {
                return OriginalOffsetX;
            }
            return -(Width + OriginalOffsetX);
        }
    }

    public int OffsetY
    {
        get
        {
            if (!FlipV)
            {
                return OriginalOffsetY;
            }
            return -(Height + OriginalOffsetY);
        }
    }

    public int OriginalOffsetX { get; private set; }

    public int OriginalOffsetY { get; private set; }
}
