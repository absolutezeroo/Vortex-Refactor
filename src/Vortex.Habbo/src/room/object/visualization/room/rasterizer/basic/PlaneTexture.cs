using System.Linq;

using Godot;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneTexture
public class PlaneTexture
{
    private List<PlaneTextureBitmap>? _bitmaps = [];

    public void Dispose()
    {
        if (_bitmaps == null)
        {
            return;
        }

        foreach (PlaneTextureBitmap bitmap in _bitmaps)
        {
            bitmap.Dispose();
        }

        _bitmaps = null;
    }

    public void AddBitmap(
        Image? bitmap,
        double normalMinX = -1, double normalMaxX = 1,
        double normalMinY = -1, double normalMaxY = 1,
        string? assetName = null)
    {
        _bitmaps?.Add(new PlaneTextureBitmap(bitmap, normalMinX, normalMaxX, normalMinY, normalMaxY, assetName));
    }

    public Image? GetBitmap(IVector3d? normal)
    {
        PlaneTextureBitmap? texBitmap = GetPlaneTextureBitmap(normal);

        return texBitmap?.Bitmap;
    }

    public PlaneTextureBitmap? GetPlaneTextureBitmap(IVector3d? normal)
    {
        if (normal == null || _bitmaps == null)
        {
            return null;
        }

        return _bitmaps.FirstOrDefault(bitmap =>
            normal.X >= bitmap.NormalMinX && normal.X <= bitmap.NormalMaxX && normal.Y >= bitmap.NormalMinY &&
            normal.Y <= bitmap.NormalMaxY);
    }

    public string? GetAssetName(IVector3d? normal)
    {
        PlaneTextureBitmap? texBitmap = GetPlaneTextureBitmap(normal);

        return texBitmap?.AssetName;
    }
}
