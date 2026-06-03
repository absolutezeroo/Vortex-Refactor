using Godot;

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Room.Object.Visualization.Room.Rasterizer.Animated;

/// @see com.sulake.habbo.room.object.visualization.room.rasterizer.animated.AnimationItem
public class AnimationItem(double x, double y, double speedX, double speedY, Image? bitmapData)
    : IDisposable
{
    private readonly double _x = double.IsNaN(x) ? 0 : x;
    private readonly double _y = double.IsNaN(y) ? 0 : y;
    private readonly double _speedX = double.IsNaN(speedX) ? 0 : speedX;
    private readonly double _speedY = double.IsNaN(speedY) ? 0 : speedY;

    public Image? BitmapData { get; private set; } = bitmapData;

    public bool Disposed { get; private set; }

    bool IDisposable.disposed => Disposed;

    public void Dispose()
    {
        Disposed = true;
        BitmapData = null;
    }

    public Vector2I GetPosition(int tileWidth, int tileHeight, double scrollX, double scrollY, int timeMs)
    {
        double posX = _x;
        double posY = _y;

        if (scrollX > 0)
        {
            posX += _speedX / scrollX * timeMs / 1000;
        }

        if (scrollY > 0)
        {
            posY += _speedY / scrollY * timeMs / 1000;
        }

        int pixelX = (int)(posX % 1 * tileWidth);
        int pixelY = (int)(posY % 1 * tileHeight);

        return new Vector2I(pixelX, pixelY);
    }
}
