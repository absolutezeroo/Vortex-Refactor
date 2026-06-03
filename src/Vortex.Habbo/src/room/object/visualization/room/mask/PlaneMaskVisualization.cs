using Vortex.Room.Object.Visualization.Utils;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Mask;

/// @see com.sulake.habbo.room.object.visualization.room.mask.PlaneMaskVisualization
public class PlaneMaskVisualization
{
    private List<PlaneMaskBitmap>? _bitmaps = [];

    public void Dispose()
    {
        if (_bitmaps == null)
        {
            return;
        }

        foreach (PlaneMaskBitmap bitmap in _bitmaps)
        {
            bitmap.Dispose();
        }

        _bitmaps = null;
    }

    public void AddBitmap(IGraphicAsset asset,
        double normalMinX = PlaneMaskBitmap.MIN_NORMAL_COORDINATE_VALUE,
        double normalMaxX = PlaneMaskBitmap.MAX_NORMAL_COORDINATE_VALUE,
        double normalMinY = PlaneMaskBitmap.MIN_NORMAL_COORDINATE_VALUE,
        double normalMaxY = PlaneMaskBitmap.MAX_NORMAL_COORDINATE_VALUE)
    {
        PlaneMaskBitmap bitmap = new(asset, normalMinX, normalMaxX, normalMinY, normalMaxY);
        _bitmaps?.Add(bitmap);
    }

    public IGraphicAsset? GetAsset(IVector3d? normal)
    {
        if (normal == null || _bitmaps == null)
        {
            return null;
        }

        foreach (PlaneMaskBitmap bitmap in _bitmaps)
        {
            if (normal.X >= bitmap.NormalMinX && normal.X <= bitmap.NormalMaxX &&
                normal.Y >= bitmap.NormalMinY && normal.Y <= bitmap.NormalMaxY)
            {
                return bitmap.Asset;
            }
        }

        return null;
    }
}
