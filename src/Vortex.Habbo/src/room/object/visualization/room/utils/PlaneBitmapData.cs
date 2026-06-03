using Godot;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Utils;

/// @see com.sulake.habbo.room.object.visualization.room.utils.PlaneBitmapData
public class PlaneBitmapData(Image? bitmap, int timeStamp)
{
    public Image? Bitmap { get; private set; } = bitmap;

    public int TimeStamp => timeStamp;

    public void Dispose()
    {
        Bitmap = null;
    }
}
