using System;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Data carrier for a single 3D plane in room geometry (floor, wall, landscape, billboard).
/// </summary>
/// @see com.sulake.habbo.room.object.RoomPlaneData
public class RoomPlaneData
{
    public const int TYPE_UNDEFINED = 0;
    public const int TYPE_FLOOR = 1;
    public const int TYPE_WALL = 2;
    public const int TYPE_LANDSCAPE = 3;
    public const int TYPE_BILLBOARD = 4;

    private readonly Vector3d _loc;
    private readonly Vector3d _leftSide;
    private readonly Vector3d _rightSide;
    private readonly Vector3d _normal;
    private readonly Vector3d _normalDirection;
    private readonly IVector3d[]? _secondaryNormals;
    private readonly List<RoomPlaneMaskData> _masks = [];

    public RoomPlaneData(int type, IVector3d loc, IVector3d leftSide, IVector3d rightSide, List<IVector3d>? secondaryNormals = null)
    {
        Type = type;
        _loc = new Vector3d();
        _loc.Assign(loc);
        _leftSide = new Vector3d();
        _leftSide.Assign(leftSide);
        _rightSide = new Vector3d();
        _rightSide.Assign(rightSide);

        Vector3d? cross = Vector3d.CrossProduct(_leftSide, _rightSide);
        _normal = cross ?? new Vector3d();

        // Compute normal direction (azimuth, elevation) in degrees
        double nx = _normal.X;
        double ny = _normal.Y;
        double nz = _normal.Z;
        double len = Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
        if (len > 0)
        {
            nx /= len;
            ny /= len;
            nz /= len;
        }
        double azimuth = Math.Atan2(ny, nx) * 180.0 / Math.PI;
        double elevation = Math.Asin(nz) * 180.0 / Math.PI;
        _normalDirection = new Vector3d((azimuth + 360) % 360, elevation, 0);

        if (secondaryNormals == null)
        {
            return;
        }

        _secondaryNormals = new IVector3d[secondaryNormals.Count];

        for (int i = 0;
             i < secondaryNormals.Count;
             i++)
        {
            Vector3d sn = new();
            sn.Assign(secondaryNormals[i]);
            double snLen = sn.Length;

            if (snLen > 0)
            {
                sn = new Vector3d(sn.X / snLen, sn.Y / snLen, sn.Z / snLen);
            }

            _secondaryNormals[i] = sn;
        }
    }

    public int Type { get; }

    public IVector3d Location => _loc;
    public IVector3d LeftSide => _leftSide;
    public IVector3d RightSide => _rightSide;
    public IVector3d Normal => _normal;
    public IVector3d NormalDirection => _normalDirection;
    public int MaskCount => _masks.Count;

    public int SecondaryNormalCount => _secondaryNormals?.Length ?? 0;

    public IVector3d? GetSecondaryNormal(int index)
    {
        if (_secondaryNormals != null && index >= 0 && index < _secondaryNormals.Length)
        {
            return _secondaryNormals[index];
        }

        return null;
    }

    public void AddMask(double leftSideLoc, double rightSideLoc, double leftSideLength, double rightSideLength)
    {
        _masks.Add(new RoomPlaneMaskData(leftSideLoc, rightSideLoc, leftSideLength, rightSideLength));
    }

    public double GetMaskLeftSideLoc(int index)
    {
        if (index >= 0 && index < _masks.Count)
        {
            return _masks[index].LeftSideLoc;
        }

        return 0;
    }

    public double GetMaskRightSideLoc(int index)
    {
        if (index >= 0 && index < _masks.Count)
        {
            return _masks[index].RightSideLoc;
        }

        return 0;
    }

    public double GetMaskLeftSideLength(int index)
    {
        if (index >= 0 && index < _masks.Count)
        {
            return _masks[index].LeftSideLength;
        }

        return 0;
    }

    public double GetMaskRightSideLength(int index)
    {
        if (index >= 0 && index < _masks.Count)
        {
            return _masks[index].RightSideLength;
        }

        return 0;
    }
}
