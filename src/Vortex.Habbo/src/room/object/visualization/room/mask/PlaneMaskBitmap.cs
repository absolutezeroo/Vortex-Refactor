using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Mask;

/// @see com.sulake.habbo.room.object.visualization.room.mask.PlaneMaskBitmap
public class PlaneMaskBitmap(IGraphicAsset asset,
    double normalMinX = PlaneMaskBitmap.MIN_NORMAL_COORDINATE_VALUE, double normalMaxX = PlaneMaskBitmap.MAX_NORMAL_COORDINATE_VALUE,
    double normalMinY = PlaneMaskBitmap.MIN_NORMAL_COORDINATE_VALUE, double normalMaxY = PlaneMaskBitmap.MAX_NORMAL_COORDINATE_VALUE)
{
    public const double MIN_NORMAL_COORDINATE_VALUE = -1;
    public const double MAX_NORMAL_COORDINATE_VALUE = 1;

    public IGraphicAsset? Asset { get; private set; } = asset;

    public double NormalMinX { get; } = normalMinX;

    public double NormalMaxX { get; } = normalMaxX;

    public double NormalMinY { get; } = normalMinY;

    public double NormalMaxY { get; } = normalMaxY;

    public void Dispose()
    {
        Asset = null;
    }
}
