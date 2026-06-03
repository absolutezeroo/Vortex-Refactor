// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/Matrix4x4.as

using System;

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/Matrix4x4.as
public class AvatarMatrix4x4
{
    /// @see Matrix4x4.as::const_448
    public static readonly AvatarMatrix4x4 IDENTITY = new(1, 0, 0, 0, 1, 0, 0, 0, 1);

    private const double TOLERANCE = 1e-18;

    /// @see Matrix4x4.as::Matrix4x4
    public AvatarMatrix4x4
    (
        double param1 = 0, double param2 = 0, double param3 = 0,
        double param4 = 0, double param5 = 0, double param6 = 0,
        double param7 = 0, double param8 = 0, double param9 = 0
    )
    {
        Data = [param1, param2, param3, param4, param5, param6, param7, param8, param9];
    }

    /// @see Matrix4x4.as::get data
    public double[] Data { get; private set; }

    /// @see Matrix4x4.as::getXRotationMatrix
    public static AvatarMatrix4x4 GetXRotationMatrix(double param1)
    {
        double radians = param1 * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);

        return new AvatarMatrix4x4(1, 0, 0, 0, cos, -sin, 0, sin, cos);
    }

    /// @see Matrix4x4.as::getYRotationMatrix
    public static AvatarMatrix4x4 GetYRotationMatrix(double param1)
    {
        double radians = param1 * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);

        return new AvatarMatrix4x4(cos, 0, sin, 0, 1, 0, -sin, 0, cos);
    }

    /// @see Matrix4x4.as::getZRotationMatrix
    public static AvatarMatrix4x4 GetZRotationMatrix(double param1)
    {
        double radians = param1 * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);

        return new AvatarMatrix4x4(cos, -sin, 0, sin, cos, 0, 0, 0, 1);
    }

    /// @see Matrix4x4.as::identity
    public AvatarMatrix4x4 Identity()
    {
        Data = [1, 0, 0, 0, 1, 0, 0, 0, 1];

        return this;
    }

    /// @see Matrix4x4.as::vectorMultiplication
    public AvatarVector3D VectorMultiplication(AvatarVector3D param1)
    {
        double x = (param1.X * Data[0]) + (param1.Y * Data[3]) + (param1.Z * Data[6]);
        double y = (param1.X * Data[1]) + (param1.Y * Data[4]) + (param1.Z * Data[7]);
        double z = (param1.X * Data[2]) + (param1.Y * Data[5]) + (param1.Z * Data[8]);

        return new AvatarVector3D(x, y, z);
    }

    /// @see Matrix4x4.as::multiply
    public AvatarMatrix4x4 Multiply(AvatarMatrix4x4 param1)
    {
        double[] d = param1.Data;
        double r0 = (Data[0] * d[0]) + (Data[1] * d[3]) + (Data[2] * d[6]);
        double r1 = (Data[0] * d[1]) + (Data[1] * d[4]) + (Data[2] * d[7]);
        double r2 = (Data[0] * d[2]) + (Data[1] * d[5]) + (Data[2] * d[8]);
        double r3 = (Data[3] * d[0]) + (Data[4] * d[3]) + (Data[5] * d[6]);
        double r4 = (Data[3] * d[1]) + (Data[4] * d[4]) + (Data[5] * d[7]);
        double r5 = (Data[3] * d[2]) + (Data[4] * d[5]) + (Data[5] * d[8]);
        double r6 = (Data[6] * d[0]) + (Data[7] * d[3]) + (Data[8] * d[6]);
        double r7 = (Data[6] * d[1]) + (Data[7] * d[4]) + (Data[8] * d[7]);
        double r8 = (Data[6] * d[2]) + (Data[7] * d[5]) + (Data[8] * d[8]);

        return new AvatarMatrix4x4(r0, r1, r2, r3, r4, r5, r6, r7, r8);
    }

    /// @see Matrix4x4.as::scalarMultiply
    public void ScalarMultiply(double param1)
    {
        for (int i = 0;
             i < Data.Length;
             i++)
        {
            Data[i] *= param1;
        }
    }

    /// @see Matrix4x4.as::rotateX
    public AvatarMatrix4x4 RotateX(double param1)
    {
        double radians = param1 * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);
        AvatarMatrix4x4 m = new(1, 0, 0, 0, cos, -sin, 0, sin, cos);
        return m.Multiply(this);
    }

    /// @see Matrix4x4.as::rotateY
    public AvatarMatrix4x4 RotateY(double param1)
    {
        double radians = param1 * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);
        AvatarMatrix4x4 m = new(cos, 0, sin, 0, 1, 0, -sin, 0, cos);

        return m.Multiply(this);
    }

    /// @see Matrix4x4.as::rotateZ
    public AvatarMatrix4x4 RotateZ(double param1)
    {
        double radians = param1 * Math.PI / 180.0;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);
        AvatarMatrix4x4 m = new(cos, -sin, 0, sin, cos, 0, 0, 0, 1);

        return m.Multiply(this);
    }

    /// @see Matrix4x4.as::transpose
    public AvatarMatrix4x4 Transpose()
    {
        return new AvatarMatrix4x4(
            Data[0], Data[3], Data[6],
            Data[1], Data[4], Data[7],
            Data[2], Data[5], Data[8]
        );
    }

    /// @see Matrix4x4.as::equals
    public static bool Equals(AvatarMatrix4x4 param1)
    {
        return false;
    }
}
