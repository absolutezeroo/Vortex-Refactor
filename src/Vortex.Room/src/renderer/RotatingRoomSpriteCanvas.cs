using System;

using Vortex.Room.Utils;

namespace Vortex.Room.Renderer;

/// <summary>
/// Extended room canvas with camera rotation, shaking effects, and debug tools.
/// </summary>
/// @see com.sulake.room.renderer.RotatingRoomSpriteCanvas (class_3656)
public class RotatingRoomSpriteCanvas(IRoomSpriteCanvasContainer container, int id, int width, int height, int scale)
    : RoomSpriteCanvas(container, id, width, height, scale)
{
    private bool _shaking;
    private double _rotationSpeed;
    private Vector3d? _rotationOrigin;
    private double _rotationRodLength;
    private Vector3d? _savedDirection;
    private Vector3d? _savedLocation;
    private int _shakeFrame;

    public override void Render(int time, bool update = false)
    {
        DoMagic();
        base.Render(time, update);
    }

    private RoomGeometry? GetGeometry()
    {
        return Geometry as RoomGeometry;
    }

    private void DoMagic()
    {
        if (_rotationSpeed != 0)
        {
            Vector3d dir = _savedDirection!;
            GetGeometry()!.Direction = new Vector3d(dir.X + _rotationSpeed, dir.Y, dir.Z);
            IVector3d newDir = GetGeometry()!.Direction;
            GetGeometry()!.SetDepthVector(new Vector3d(newDir.X, newDir.Y, 5));

            Vector3d loc = new();
            loc.Assign(_rotationOrigin!);
            loc.X += _rotationRodLength * Math.Cos((newDir.X + 180) / 180.0 * Math.PI) * Math.Cos(newDir.Y / 180.0 * Math.PI);
            loc.Y += _rotationRodLength * Math.Sin((newDir.X + 180) / 180.0 * Math.PI) * Math.Cos(newDir.Y / 180.0 * Math.PI);
            loc.Z += _rotationRodLength * Math.Sin(newDir.Y / 180.0 * Math.PI);
            GetGeometry()!.Location = loc;

            _savedLocation = new Vector3d();
            _savedLocation.Assign(loc);
            _savedDirection = new Vector3d();
            _savedDirection.Assign(GetGeometry()!.Direction);
        }

        if (RoomShakingEffect.IsVisualizationOn() && !_shaking)
        {
            ChangeShaking();
        }
        else if (!RoomShakingEffect.IsVisualizationOn() && _shaking)
        {
            ChangeShaking();
        }

        if (RoomRotatingEffect.IsVisualizationOn())
        {
            ChangeRotation();
        }

        if (_shaking)
        {
            _shakeFrame++;
            Vector3d dir = _savedDirection!;
            Vector3d? shakeDir = Vector3d.Sum(dir, new Vector3d(
                Math.Sin(_shakeFrame * 5 / 180.0 * Math.PI) * 2,
                Math.Sin(_shakeFrame / 180.0 * Math.PI) * 5,
                Math.Sin(_shakeFrame * 10 / 180.0 * Math.PI) * 2
            ));
            if (shakeDir != null)
            {
                GetGeometry()!.Direction = shakeDir;
            }
        }
        else
        {
            _shakeFrame = 0;
            if (_savedDirection != null)
            {
                GetGeometry()!.Direction = _savedDirection;
            }
        }
    }

    private void ChangeShaking()
    {
        _shaking = !_shaking;
        if (_shaking)
        {
            IVector3d dir = GetGeometry()!.Direction;
            _savedDirection = new Vector3d(dir.X, dir.Y, dir.Z);
        }
    }

    private void ChangeRotation()
    {
        if (_shaking)
        {
            return;
        }
        if (_rotationSpeed == 0)
        {
            if (GetGeometry() != null)
            {
                IVector3d location = GetGeometry()!.Location;
                IVector3d dirAxis = GetGeometry()!.DirectionAxis;
                _savedLocation = new Vector3d();
                _savedLocation.Assign(location);
                _savedDirection = new Vector3d();
                _savedDirection.Assign(GetGeometry()!.Direction);

                IVector3d? intersection = RoomGeometry.GetIntersectionVector(
                    location, dirAxis,
                    new Vector3d(0, 0, 0), new Vector3d(0, 0, 1));
                if (intersection != null)
                {
                    _rotationOrigin = new Vector3d(intersection.X, intersection.Y, intersection.Z);
                    _rotationRodLength = Vector3d.Dif(intersection, location)!.Length;
                    _rotationSpeed = 1;
                }
            }
        }
        else
        {
            _rotationSpeed = 0;
            if (GetGeometry() != null && _savedLocation != null && _savedDirection != null)
            {
                GetGeometry()!.Location = _savedLocation;
                GetGeometry()!.Direction = _savedDirection;
                GetGeometry()!.SetDepthVector(new Vector3d(_savedDirection.X, _savedDirection.Y, 5));
            }
        }
    }
}
