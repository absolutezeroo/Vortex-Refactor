// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/Vector3D.as

using System;

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/Vector3D.as
public class AvatarVector3D
{
    /// @see Vector3D.as::Vector3D
    public AvatarVector3D(double param1 = 0, double param2 = 0, double param3 = 0)
    {
        X = param1;
        Y = param2;
        Z = param3;
    }

    /// @see Vector3D.as::get x
    public double X { get; set; }

    /// @see Vector3D.as::get y
    public double Y { get; set; }

    /// @see Vector3D.as::get z
    public double Z { get; set; }

    /// @see Vector3D.as::dot (static)
    public static double Dot(AvatarVector3D param1, AvatarVector3D param2)
    {
        return (param1.X * param2.X) + (param1.Y * param2.Y) + (param1.Z * param2.Z);
    }

    /// @see Vector3D.as::cross (static)
    public static AvatarVector3D Cross(AvatarVector3D param1, AvatarVector3D param2)
    {
        AvatarVector3D result = new()
        {
            X = (param1.Y * param2.Z) - (param1.Z * param2.Y),
            Y = (param1.Z * param2.X) - (param1.X * param2.Z),
            Z = (param1.X * param2.Y) - (param1.Y * param2.X),
        };
        return result;
    }

    /// @see Vector3D.as::subtract (static)
    public static AvatarVector3D Subtract(AvatarVector3D param1, AvatarVector3D param2)
    {
        return new AvatarVector3D(param1.X - param2.X, param1.Y - param2.Y, param1.Z - param2.Z);
    }

    /// @see Vector3D.as::dot (instance)
    public double Dot(AvatarVector3D param1)
    {
        return (X * param1.X) + (Y * param1.Y) + (Z * param1.Z);
    }

    /// @see Vector3D.as::cross (instance)
    public AvatarVector3D Cross(AvatarVector3D param1)
    {
        AvatarVector3D result = new()
        {
            X = (Y * param1.Z) - (Z * param1.Y),
            Y = (Z * param1.X) - (X * param1.Z),
            Z = (X * param1.Y) - (Y * param1.X),
        };
        return result;
    }

    /// @see Vector3D.as::subtract (instance, mutating)
    public void Subtract(AvatarVector3D param1)
    {
        X -= param1.X;
        Y -= param1.Y;
        Z -= param1.Z;
    }

    /// @see Vector3D.as::add
    public void Add(AvatarVector3D param1)
    {
        X += param1.X;
        Y += param1.Y;
        Z += param1.Z;
    }

    /// @see Vector3D.as::normalize
    public void Normalize()
    {
        double invLength = 1.0 / Length();
        X *= invLength;
        Y *= invLength;
        Z *= invLength;
    }

    /// @see Vector3D.as::length
    public double Length()
    {
        return Math.Sqrt((X * X) + (Y * Y) + (Z * Z));
    }

    /// @see Vector3D.as::toString
    public override string ToString()
    {
        return "Vector3D: (" + X + "," + Y + "," + Z + ")";
    }
}
