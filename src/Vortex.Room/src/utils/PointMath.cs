using Godot;

namespace Vortex.Room.Utils;

/// <summary>
/// Static 2D point arithmetic helpers.
/// </summary>
/// @see com.sulake.room.utils.PointMath
public static class PointMath
{
    public static Vector2 Sum(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2 Sub(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2 Mul(Vector2 a, float scalar)
    {
        return new Vector2(a.X * scalar, a.Y * scalar);
    }
}
