using System.Linq;

using Godot;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneMaterial
public class PlaneMaterial
{
    private List<PlaneMaterialCellMatrix>? _items = [];
    private bool _isCached;

    public void Dispose()
    {
        if (_items == null)
        {
            return;
        }

        foreach (PlaneMaterialCellMatrix item in _items)
        {
            item.Dispose();
        }

        _items = null;
    }

    public void ClearCache()
    {
        if (!_isCached || _items == null)
        {
            return;
        }

        foreach (PlaneMaterialCellMatrix item in _items)
        {
            item.ClearCache();
        }

        _isCached = false;
    }

    public PlaneMaterialCellMatrix AddMaterialCellMatrix(
        int columnCount, int repeatMode = PlaneMaterialCellMatrix.REPEAT_MODE_ALL,
        int align = PlaneMaterialCellMatrix.ALIGN_DEFAULT,
        double normalMinX = -1, double normalMaxX = 1,
        double normalMinY = -1, double normalMaxY = 1)
    {
        PlaneMaterialCellMatrix matrix = new(columnCount, repeatMode, align, normalMinX, normalMaxX, normalMinY, normalMaxY);

        _items?.Add(matrix);

        return matrix;
    }

    public PlaneMaterialCellMatrix? GetMaterialCellMatrix(IVector3d? normal)
    {
        if (normal == null || _items == null)
        {
            return null;
        }

        return _items.FirstOrDefault(item =>
            normal.X >= item.NormalMinX && normal.X <= item.NormalMaxX && normal.Y >= item.NormalMinY && normal.Y <= item.NormalMaxY);
    }

    public Image? Render(
        Image? canvas, int width, int height,
        IVector3d normal, bool useTexture,
        int offsetX, int offsetY, bool topAligned)
    {
        if (width < 1)
        {
            width = 1;
        }

        if (height < 1)
        {
            height = 1;
        }

        PlaneMaterialCellMatrix? matrix = GetMaterialCellMatrix(normal);

        if (matrix == null)
        {
            return null;
        }

        _isCached = true;

        return matrix.Render(canvas, width, height, normal, useTexture, offsetX, offsetY, topAligned);
    }
}
