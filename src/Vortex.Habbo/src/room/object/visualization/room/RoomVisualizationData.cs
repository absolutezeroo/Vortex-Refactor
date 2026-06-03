using System.Xml.Linq;

using Vortex.Habbo.Room.Object.Visualization.Room.Mask;
using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer;
using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Animated;
using Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room;

/// @see com.sulake.habbo.room.object.visualization.room.RoomVisualizationData
public class RoomVisualizationData : IRoomObjectVisualizationData
{
    private WallRasterizer? _wallRasterizer = new();
    private FloorRasterizer? _floorRasterizer = new();
    private LandscapeRasterizer? _landscapeRasterizer = new();

    public bool Initialized { get; private set; }

    public IPlaneRasterizer? FloorRasterizer => _floorRasterizer;

    public IPlaneRasterizer? WallRasterizer => _wallRasterizer;

    public WallAdRasterizer? WallAdRasterizer { get; private set; } = new();

    public IPlaneRasterizer? LandscapeRasterizer => _landscapeRasterizer;

    public PlaneMaskManager? MaskManager { get; private set; } = new();

    public void Dispose()
    {
        if (_wallRasterizer != null)
        {
            _wallRasterizer.Dispose();
            _wallRasterizer = null;
        }

        if (_floorRasterizer != null)
        {
            _floorRasterizer.Dispose();
            _floorRasterizer = null;
        }

        if (WallAdRasterizer != null)
        {
            WallAdRasterizer.Dispose();
            WallAdRasterizer = null;
        }

        if (_landscapeRasterizer != null)
        {
            _landscapeRasterizer.Dispose();
            _landscapeRasterizer = null;
        }

        if (MaskManager != null)
        {
            MaskManager.Dispose();
            MaskManager = null;
        }
    }

    public void ClearCache()
    {
        _wallRasterizer?.ClearCache();
        _floorRasterizer?.ClearCache();
        _landscapeRasterizer?.ClearCache();
    }

    public bool Initialize(XElement xml)
    {
        Reset();

        XElement? wallData = xml.Element("wallData");

        if (wallData != null)
        {
            _wallRasterizer?.Initialize(wallData);
        }

        XElement? floorData = xml.Element("floorData");

        if (floorData != null)
        {
            _floorRasterizer?.Initialize(floorData);
        }

        XElement? wallAdData = xml.Element("wallAdData");

        if (wallAdData != null)
        {
            WallAdRasterizer?.Initialize(wallAdData);
        }

        XElement? landscapeData = xml.Element("landscapeData");

        if (landscapeData != null)
        {
            _landscapeRasterizer?.Initialize(landscapeData);
        }

        XElement? maskData = xml.Element("maskData");

        if (maskData != null)
        {
            MaskManager?.Initialize(maskData);
        }

        return true;
    }

    public void InitializeAssetCollection(IGraphicAssetCollection? assetCollection)
    {
        if (Initialized)
        {
            return;
        }

        _wallRasterizer?.InitializeAssetCollection(assetCollection);
        _floorRasterizer?.InitializeAssetCollection(assetCollection);
        WallAdRasterizer?.InitializeAssetCollection(assetCollection);
        _landscapeRasterizer?.InitializeAssetCollection(assetCollection);
        MaskManager?.InitializeAssetCollection(assetCollection);

        Initialized = true;
    }

    protected void Reset()
    {
    }
}
