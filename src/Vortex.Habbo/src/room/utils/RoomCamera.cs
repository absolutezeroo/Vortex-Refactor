using System;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Utils;

/// @see com.sulake.habbo.room.utils.RoomCamera
public class RoomCamera
{
    private const double MOVE_SPEED_DENOMINATOR = 12.0;

    private Vector3d? _targetPosition;
    private double _totalDistance;
    private double _currentDistance;
    private bool _isStarting;
    private Vector3d? _location;
    private readonly Vector3d _targetObjectLoc = new();
    private int _scale;
    private bool _scaleChanged;
    private int _followDuration;

    public IVector3d? Location => _location;

    public int TargetId { get; set; } = -1;

    public int TargetCategory { get; set; } = -2;

    public IVector3d TargetObjectLoc => _targetObjectLoc;

    public void SetTargetObjectLoc(IVector3d value)
    {
        _targetObjectLoc.Assign(value);
    }

    public bool LimitedLocationX { get; set; }

    public bool LimitedLocationY { get; set; }

    public bool CenteredLocX { get; set; }

    public bool CenteredLocY { get; set; }

    public int ScreenWd { get; set; }

    public int ScreenHt { get; set; }

    public int Scale
    {
        get => _scale;
        set
        {
            if (_scale != value)
            {
                _scale = value;
                _scaleChanged = true;
            }
        }
    }

    public int RoomWd { get; set; }

    public int RoomHt { get; set; }

    public int GeometryUpdateId { get; set; } = -1;

    public bool IsMoving => _targetPosition != null && _location != null;

    /// @see com.sulake.habbo.room.utils.RoomCamera::set target
    public IVector3d? Target
    {
        set
        {
            if (value == null)
            {
                return;
            }

            if (_targetPosition == null)
            {
                _targetPosition = new Vector3d();
            }

            if (_targetPosition.X != value.X || _targetPosition.Y != value.Y || _targetPosition.Z != value.Z)
            {
                _targetPosition.Assign(value);

                Vector3d? diff = Vector3d.Dif(_targetPosition, _location);

                if (diff != null)
                {
                    _totalDistance = diff.Length;
                }

                _isStarting = true;
            }
        }
    }

    public void Dispose()
    {
        _targetPosition = null;
        _location = null;
    }

    /// @see com.sulake.habbo.room.utils.RoomCamera::initializeLocation
    public void InitializeLocation(IVector3d location)
    {
        if (_location != null)
        {
            return;
        }

        _location = new Vector3d();
        _location.Assign(location);
    }

    /// @see com.sulake.habbo.room.utils.RoomCamera::resetLocation
    public void ResetLocation(IVector3d location)
    {
        if (_location == null)
        {
            _location = new Vector3d();
        }

        _location.Assign(location);
    }

    /// @see com.sulake.habbo.room.utils.RoomCamera::update
    public void Update(uint followDuration, double moveSpeed)
    {
        if (_followDuration <= 0 || _targetPosition == null || _location == null)
        {
            return;
        }

        if (_scaleChanged)
        {
            _scaleChanged = false;
            _location = _targetPosition;
            _targetPosition = null;
            return;
        }

        Vector3d? diff = Vector3d.Dif(_targetPosition, _location);

        if (diff == null)
        {
            return;
        }

        if (diff.Length > _totalDistance)
        {
            _totalDistance = diff.Length;
        }

        if (diff.Length <= moveSpeed)
        {
            _location = _targetPosition;
            _targetPosition = null;
            _currentDistance = 0;
        }
        else
        {
            double sinValue = Math.Sin(Math.PI * diff.Length / _totalDistance);
            double halfSpeed = moveSpeed * 0.5;
            double maxSpeed = _totalDistance / MOVE_SPEED_DENOMINATOR;
            double speed = halfSpeed + ((maxSpeed - halfSpeed) * sinValue);

            if (_isStarting)
            {
                if (speed < _currentDistance)
                {
                    speed = _currentDistance;

                    if (speed > diff.Length)
                    {
                        speed = diff.Length;
                    }
                }
                else
                {
                    _isStarting = false;
                }
            }

            _currentDistance = speed;
            diff.Div(diff.Length);
            diff.Mul(speed);
            _location = Vector3d.Sum(_location, diff);
        }
    }

    public void Reset()
    {
        GeometryUpdateId = -1;
    }

    /// @see com.sulake.habbo.room.utils.RoomCamera::activateFollowing
    public void ActivateFollowing(int duration)
    {
        _followDuration = duration;
    }
}
