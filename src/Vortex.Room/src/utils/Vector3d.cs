using System;

namespace Vortex.Room.Utils;

/// <summary>
/// Mutable 3D vector with lazy length calculation.
/// </summary>
/// @see com.sulake.room.utils.Vector3d
public class Vector3d(double x = 0, double y = 0, double z = 0) : IVector3d
{
    private double _x = x;
    private double _y = y;
    private double _z = z;
    private double _length = double.NaN;

    public static Vector3d? Sum(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        return new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3d? Dif(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        return new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3d? Product(IVector3d? a, double scalar)
    {
        if (a == null)
        {
            return null;
        }

        return new Vector3d(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static double DotProduct(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return 0;
        }

        return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
    }

    public static Vector3d? CrossProduct(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return null;
        }

        return new Vector3d(
            (a.Y * b.Z) - (a.Z * b.Y),
            (a.Z * b.X) - (a.X * b.Z),
            (a.X * b.Y) - (a.Y * b.X)
        );
    }

    public static double ScalarProjection(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return -1;
        }

        double len = b.Length;

        if (len > 0)
        {

            return ((a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z)) / len;
        }
        return -1;
    }

    public static double CosAngle(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return 0;
        }

        double len = a.Length * b.Length;

        if (len == 0)
        {
            return 0;
        }

        return DotProduct(a, b) / len;
    }

    public static bool IsEqual(IVector3d? a, IVector3d? b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }

    public double X
    {
        get => _x;
        set
        {
            _x = value;
            _length = double.NaN;
        }
    }

    public double Y
    {
        get => _y;
        set
        {
            _y = value;
            _length = double.NaN;
        }
    }

    public double Z
    {
        get => _z;
        set
        {
            _z = value;
            _length = double.NaN;
        }
    }

    public double Length
    {
        get
        {
            if (double.IsNaN(_length))
            {
                _length = Math.Sqrt((_x * _x) + (_y * _y) + (_z * _z));
            }

            return _length;
        }
    }

    public void Negate()
    {
        _x = -_x;
        _y = -_y;
        _z = -_z;
    }

    public void Add(IVector3d? other)
    {
        if (other == null)
        {
            return;
        }
        _x += other.X;
        _y += other.Y;
        _z += other.Z;
        _length = double.NaN;
    }

    public void Sub(IVector3d? other)
    {
        if (other == null)
        {
            return;
        }
        _x -= other.X;
        _y -= other.Y;
        _z -= other.Z;
        _length = double.NaN;
    }

    public void Mul(double scalar)
    {
        _x *= scalar;
        _y *= scalar;
        _z *= scalar;
        _length = double.NaN;
    }

    public void Div(double scalar)
    {
        if (scalar == 0)
        {
            return;
        }

        _x /= scalar;
        _y /= scalar;
        _z /= scalar;
        _length = double.NaN;
    }

    public void Assign(IVector3d? other)
    {
        if (other == null)
        {
            return;
        }

        _x = other.X;
        _y = other.Y;
        _z = other.Z;
        _length = double.NaN;
    }

    public override string ToString()
    {
        return $"({_x},{_y},{_z})";
    }
}
