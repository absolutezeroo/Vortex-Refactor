using Godot;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Basic;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.basic.PlaneTextureBitmap
public class PlaneTextureBitmap(Image? bitmap,
    double normalMinX = -1, double normalMaxX = 1,
    double normalMinY = -1, double normalMaxY = 1,
    string? assetName = null)
{
    public const double MIN_NORMAL_COORDINATE_VALUE = -1;
    public const double MAX_NORMAL_COORDINATE_VALUE = 1;

    public Image? Bitmap { get; private set; } = bitmap;

    public string? AssetName => assetName;

    public double NormalMinX => normalMinX;

    public double NormalMaxX => normalMaxX;

    public double NormalMinY => normalMinY;

    public double NormalMaxY => normalMaxY;

    public void Dispose()
    {
        Bitmap = null;
    }
}
