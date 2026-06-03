using System;

using Godot;

namespace Vortex.Room.Utils;

/// <summary>
/// Isometric room coordinate projection with Euler angle-based rotation matrices.
/// Converts 3D room coordinates to 2D screen positions and back.
/// </summary>
/// @see com.sulake.room.utils.RoomGeometry
public class RoomGeometry : IRoomGeometry
{
    public const double SCALE_ZOOMED_IN = 64;
    public const double SCALE_ZOOMED_OUT = 32;

    private readonly Vector3d _xAxis;
    private readonly Vector3d _yAxis;
    private readonly Vector3d _zAxis;
    private readonly Vector3d _directionAxis;
    private readonly Vector3d _locationBuffer;
    private readonly Vector3d _directionBuffer;
    private readonly Vector3d _depth;
    private double _scaleFactor = 1;
    private double _xScale = 1;
    private double _yScale = 1;
    private double _zScale = 1;
    private readonly double _xScaleInternal = 1;
    private readonly double _yScaleInternal = 1;
    private readonly double _zScaleInternal = 1;
    private Vector3d? _location;
    private Vector3d? _rawDirection;
    private readonly double _depthNear = -500;
    private readonly double _depthFar = 500;
    private Dictionary<string, Vector3d>? _displacements;

    public RoomGeometry(double scale, IVector3d direction, IVector3d location, IVector3d? depthDirection = null)
    {
        _xAxis = new Vector3d();
        _yAxis = new Vector3d();
        _zAxis = new Vector3d();
        _directionAxis = new Vector3d();
        _locationBuffer = new Vector3d();
        _directionBuffer = new Vector3d();
        _depth = new Vector3d();
        _xScaleInternal = 1;
        _yScaleInternal = 1;
        XScale = 1;
        YScale = 1;
        _zScaleInternal = Math.Sqrt(0.5) / Math.Sqrt(0.75);
        ZScale = 1;
        Scale = scale;
        Location = new Vector3d(location.X, location.Y, location.Z);
        Direction = new Vector3d(direction.X, direction.Y, direction.Z);

        if (depthDirection != null)
        {
            SetDepthVector(depthDirection);
        }
        else
        {
            SetDepthVector(direction);
        }

        _displacements = new Dictionary<string, Vector3d>();
    }

    public static IVector3d? GetIntersectionVector(IVector3d origin, IVector3d direction, IVector3d planeOrigin, IVector3d planeNormal)
    {
        double denom = Vector3d.DotProduct(direction, planeNormal);

        if (Math.Abs(denom) < 0.00001)
        {
            return null;
        }

        Vector3d? diff = Vector3d.Dif(origin, planeOrigin);
        double t = -Vector3d.DotProduct(planeNormal, diff) / denom;

        return Vector3d.Sum(origin, Vector3d.Product(direction, t));
    }

    public int UpdateId { get; private set; }

    public double Scale
    {
        get => _scaleFactor / Math.Sqrt(0.5);
        set
        {
            double s = value;

            if (s <= 1)
            {
                s = 1;
            }

            s *= Math.Sqrt(0.5);

            if (s == _scaleFactor)
            {
                return;
            }

            _scaleFactor = s;
            UpdateId++;
        }
    }

    public IVector3d DirectionAxis => _directionAxis;

    public IVector3d LocationValue
    {
        get
        {
            _locationBuffer.Assign(_location!);
            _locationBuffer.X *= _xScale;
            _locationBuffer.Y *= _yScale;
            _locationBuffer.Z *= _zScale;

            return _locationBuffer;
        }
    }

    public IVector3d Direction
    {
        get => _directionBuffer;
        set
        {
            if (value == null)
            {
                return;
            }

            _rawDirection ??= new Vector3d();

            double oldX = _rawDirection.X;
            double oldY = _rawDirection.Y;
            double oldZ = _rawDirection.Z;
            _rawDirection.Assign(value);
            _directionBuffer.Assign(value);

            if (_rawDirection.X != oldX || _rawDirection.Y != oldY || _rawDirection.Z != oldZ)
            {
                UpdateId++;
            }

            Vector3d axisY = new(0, 1, 0);
            Vector3d axisZ = new(0, 0, 1);
            Vector3d axisX = new(1, 0, 0);

            double radX = value.X / 180.0 * Math.PI;
            double radY = value.Y / 180.0 * Math.PI;
            double radZ = value.Z / 180.0 * Math.PI;

            double cosX = Math.Cos(radX);
            double sinX = Math.Sin(radX);

            Vector3d rotY = Vector3d.Sum(Vector3d.Product(axisY, cosX), Vector3d.Product(axisX, -sinX))!;
            Vector3d rotZ = new(axisZ.X, axisZ.Y, axisZ.Z);
            Vector3d rotX = Vector3d.Sum(Vector3d.Product(axisY, sinX), Vector3d.Product(axisX, cosX))!;

            double cosY = Math.Cos(radY);
            double sinY = Math.Sin(radY);

            Vector3d finalX = new(rotY.X, rotY.Y, rotY.Z);
            Vector3d finalZ = Vector3d.Sum(Vector3d.Product(rotZ, cosY), Vector3d.Product(rotX, sinY))!;
            Vector3d finalZNeg = Vector3d.Sum(Vector3d.Product(rotZ, -sinY), Vector3d.Product(rotX, cosY))!;

            if (radZ != 0)
            {
                double cosZ = Math.Cos(radZ);
                double sinZ = Math.Sin(radZ);
                Vector3d rX = Vector3d.Sum(Vector3d.Product(finalX, cosZ), Vector3d.Product(finalZ, sinZ))!;
                Vector3d rY = Vector3d.Sum(Vector3d.Product(finalX, -sinZ), Vector3d.Product(finalZ, cosZ))!;
                _xAxis.Assign(rX);
                _yAxis.Assign(rY);
                _zAxis.Assign(finalZNeg);
                _directionAxis.Assign(_zAxis);
            }
            else
            {
                _xAxis.Assign(finalX);
                _yAxis.Assign(finalZ);
                _zAxis.Assign(finalZNeg);
                _directionAxis.Assign(_zAxis);
            }
        }
    }

    public double XScale
    {
        set
        {
            double newScale = value * _xScaleInternal;

            if (_xScale == newScale)
            {
                return;
            }

            _xScale = newScale;
            UpdateId++;
        }
    }

    public double YScale
    {
        set
        {
            double newScale = value * _yScaleInternal;

            if (_yScale == newScale)
            {
                return;
            }

            _yScale = newScale;
            UpdateId++;
        }
    }

    public double ZScale
    {
        set
        {
            double newScale = value * _zScaleInternal;

            if (_zScale == newScale)
            {
                return;
            }

            _zScale = newScale;
            UpdateId++;
        }
    }

    double IRoomGeometry.ZScale
    {
        set => ZScale = value;
    }

    public IVector3d Location
    {
        get => _location!;
        set
        {
            if (value == null)
            {
                return;
            }

            _location ??= new Vector3d();

            double oldX = _location.X;
            double oldY = _location.Y;
            double oldZ = _location.Z;
            _location.Assign(value);
            _location.X /= _xScale;
            _location.Y /= _yScale;
            _location.Z /= _zScale;

            if (_location.X != oldX || _location.Y != oldY || _location.Z != oldZ)
            {
                UpdateId++;
            }
        }
    }

    public void Dispose()
    {
        _displacements?.Clear();
        _displacements = null;
    }

    public void SetDisplacement(IVector3d position, IVector3d displacement)
    {
        if (position == null || displacement == null)
        {
            return;
        }

        if (_displacements == null)
        {
            return;
        }

        string key = (int)Math.Round(position.X) + "_" + (int)Math.Round(position.Y) + "_" + (int)Math.Round(position.Z);
        Vector3d vec = new();
        vec.Assign(displacement);
        _displacements[key] = vec;
        UpdateId++;
    }

    private IVector3d? GetDisplacement(IVector3d position)
    {
        if (_displacements == null)
        {
            return null;
        }

        string key = (int)Math.Round(position.X) + "_" + (int)Math.Round(position.Y) + "_" + (int)Math.Round(position.Z);
        _displacements.TryGetValue(key, out Vector3d? vec);

        return vec;
    }

    public void SetDepthVector(IVector3d direction)
    {
        Vector3d axisY = new(0, 1, 0);
        Vector3d axisZ = new(0, 0, 1);
        Vector3d axisX = new(1, 0, 0);

        double radX = direction.X / 180.0 * Math.PI;
        double radY = direction.Y / 180.0 * Math.PI;
        double radZ = direction.Z / 180.0 * Math.PI;

        double cosX = Math.Cos(radX);
        double sinX = Math.Sin(radX);

        Vector3d rotY = Vector3d.Sum(Vector3d.Product(axisY, cosX), Vector3d.Product(axisX, -sinX))!;
        Vector3d rotZ = new(axisZ.X, axisZ.Y, axisZ.Z);
        Vector3d rotX = Vector3d.Sum(Vector3d.Product(axisY, sinX), Vector3d.Product(axisX, cosX))!;

        double cosY = Math.Cos(radY);
        double sinY = Math.Sin(radY);

        _ = new
        Vector3d(rotY.X, rotY.Y, rotY.Z);

        _ = Vector3d.Sum(Vector3d.Product(rotZ, cosY), Vector3d.Product(rotX, sinY))!;
        Vector3d finalZNeg = Vector3d.Sum(Vector3d.Product(rotZ, -sinY), Vector3d.Product(rotX, cosY))!;

        if (radZ != 0)
        {
            _ = Math.Cos(radZ);

            _ = Math.Sin(radZ);
            Vector3d depthVec = new(finalZNeg.X, finalZNeg.Y, finalZNeg.Z);
            _depth.Assign(depthVec);
        }
        else
        {
            _depth.Assign(finalZNeg);
        }

        UpdateId++;
    }

    public void AdjustLocation(IVector3d location, double amount)
    {
        if (location == null)
        {
            return;
        }

        Vector3d? offset = Vector3d.Product(_zAxis, -amount);

        if (offset == null)
        {
            return;
        }

        Location = new Vector3d(location.X + offset.X, location.Y + offset.Y, location.Z + offset.Z);
    }

    public IVector3d? GetCoordinatePosition(IVector3d position)
    {
        if (position == null)
        {
            return null;
        }

        double x = Vector3d.ScalarProjection(position, _xAxis);
        double y = Vector3d.ScalarProjection(position, _yAxis);
        double z = Vector3d.ScalarProjection(position, _zAxis);
        return new Vector3d(x, y, z);
    }

    public IVector3d? GetScreenPosition(IVector3d position)
    {
        Vector3d? diff = Vector3d.Dif(position, _location);

        if (diff == null)
        {
            return null;
        }

        diff.X *= _xScale;
        diff.Y *= _yScale;
        diff.Z *= _zScale;

        double depthVal = Vector3d.ScalarProjection(diff, _depth);

        if (depthVal < _depthNear || depthVal > _depthFar)
        {
            return null;
        }

        double screenX = Vector3d.ScalarProjection(diff, _xAxis);
        double screenY = -Vector3d.ScalarProjection(diff, _yAxis);
        screenX *= _scaleFactor;
        screenY *= _scaleFactor;

        IVector3d? displacement = GetDisplacement(position);

        if (displacement != null)
        {
            diff = Vector3d.Dif(position, _location)!;
            diff.Add(displacement);
            diff.X *= _xScale;
            diff.Y *= _yScale;
            diff.Z *= _zScale;
            depthVal = Vector3d.ScalarProjection(diff, _depth);
        }

        diff.X = screenX;
        diff.Y = screenY;
        diff.Z = depthVal;
        return diff;
    }

    public Vector2? GetScreenPoint(IVector3d position)
    {
        IVector3d? screenPos = GetScreenPosition(position);

        if (screenPos == null)
        {
            return null;
        }

        return new Vector2((float)screenPos.X, (float)screenPos.Y);
    }

    public Vector2? GetPlanePosition(Vector2 screenPoint, IVector3d planeOrigin, IVector3d planeXAxis, IVector3d planeYAxis)
    {
        double sx = screenPoint.X / _scaleFactor;
        double sy = -screenPoint.Y / _scaleFactor;

        Vector3d screenDir = Vector3d.Product(_xAxis, sx)!;
        screenDir.Add(Vector3d.Product(_yAxis, sy));
        Vector3d rayOrigin = new(_location!.X * _xScale, _location.Y * _yScale, _location.Z * _zScale);
        rayOrigin.Add(screenDir);

        Vector3d rayDir = _zAxis;
        Vector3d scaledOrigin = new(planeOrigin.X * _xScale, planeOrigin.Y * _yScale, planeOrigin.Z * _zScale);
        Vector3d scaledXAxis = new(planeXAxis.X * _xScale, planeXAxis.Y * _yScale, planeXAxis.Z * _zScale);
        Vector3d scaledYAxis = new(planeYAxis.X * _xScale, planeYAxis.Y * _yScale, planeYAxis.Z * _zScale);
        Vector3d? planeNormal = Vector3d.CrossProduct(scaledXAxis, scaledYAxis);

        Vector3d intersection = new();
        IVector3d? hitPoint = GetIntersectionVector(rayOrigin, rayDir, scaledOrigin, planeNormal!);

        if (hitPoint == null)
        {
            return null;
        }

        intersection.Assign(hitPoint);
        intersection.Sub(scaledOrigin);

        double u = Vector3d.ScalarProjection(intersection, planeXAxis) / scaledXAxis.Length * planeXAxis.Length;
        double v = Vector3d.ScalarProjection(intersection, planeYAxis) / scaledYAxis.Length * planeYAxis.Length;

        return new Vector2((float)u, (float)v);
    }

    public void PerformZoom()
    {
        if (IsZoomedIn())
        {
            Scale = 32;
        }
        else
        {
            Scale = 64;
        }
    }

    public bool IsZoomedIn()
    {
        return Scale == 64;
    }

    public void PerformZoomOut()
    {
        Scale = 32;
    }

    public void PerformZoomIn()
    {
        Scale = 64;
    }
}
