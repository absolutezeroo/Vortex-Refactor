using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Object.Logic;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.MovingObjectLogic
public class MovingObjectLogic : ObjectLogicBase
{
    public const int DEFAULT_UPDATE_INTERVAL = 500;

    private static readonly Vector3d _helperVector = new();

    private Vector3d? _movementDelta = new();
    private Vector3d? _targetBase = new();
    private double _liftAmount;
    private int _changeTime;
    private int _moveUpdateInterval = 500;

    public override IRoomObjectController? Object
    {
        get => base.Object;
        set
        {
            base.Object = value;
            if (value != null)
            {
                _targetBase?.Assign(value.Location);
            }
        }
    }

    protected int LastUpdateTime { get; private set; }

    protected int MoveUpdateInterval
    {
        set
        {
            if (value <= 0)
            {
                value = 1;
            }
            _moveUpdateInterval = value;
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _targetBase = null;
        _movementDelta = null;
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage message)
    {
        if (message == null)
        {
            return;
        }

        base.ProcessUpdateMessage(message);

        RoomObjectMoveUpdateMessage? moveMsg = message as RoomObjectMoveUpdateMessage;

        if (moveMsg is
            {
                SkipPositionUpdate: true,
            })
        {
            return;
        }

        if (message.Location != null)
        {
            _targetBase?.Assign(message.Location);
        }

        if (message.Location != null && _movementDelta != null)
        {
            _movementDelta.X = 0;
            _movementDelta.Y = 0;
            _movementDelta.Z = 0;
        }

        if (moveMsg == null)
        {
            return;
        }

        if (Object == null)
        {
            return;
        }

        if (message.Location == null)
        {
            return;
        }

        IVector3d? targetLoc = moveMsg.TargetLoc;
        MoveUpdateInterval = double.IsNaN(moveMsg.AnimationTime) ? 500 : (int)moveMsg.AnimationTime;
        _changeTime = LastUpdateTime;
        _movementDelta!.Assign(targetLoc!);
        _movementDelta.Sub(_targetBase!);
    }

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectMoveEvent.SLIDE_ANIMATION];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Update(int time)
    {
        IVector3d? locationOffset = GetLocationOffset();
        IRoomObjectModelController? model = Object?.ModelController;

        if (model != null)
        {
            if (locationOffset != null)
            {
                if (_liftAmount != locationOffset.Z)
                {
                    _liftAmount = locationOffset.Z;
                    model.SetNumber("furniture_lift_amount", _liftAmount);
                }
            }
            else if (_liftAmount != 0)
            {
                _liftAmount = 0;
                model.SetNumber("furniture_lift_amount", _liftAmount);
            }
        }

        if (_movementDelta is
            {
                Length: > 0,
            } || locationOffset != null)
        {
            int elapsed = time - _changeTime;

            if (elapsed == _moveUpdateInterval >> 1)
            {
                elapsed++;
            }

            if (elapsed > _moveUpdateInterval)
            {
                elapsed = _moveUpdateInterval;
            }

            if (_movementDelta is
                {
                    Length: > 0,
                })
            {
                _helperVector.Assign(_movementDelta);
                _helperVector.Mul((double)elapsed / _moveUpdateInterval);
                _helperVector.Add(_targetBase!);
            }
            else
            {
                _helperVector.Assign(_targetBase!);
            }

            if (locationOffset != null)
            {
                _helperVector.Add(locationOffset);
            }

            Object?.SetLocation(_helperVector);

            if (elapsed == _moveUpdateInterval && _movementDelta != null)
            {
                _movementDelta.X = 0;
                _movementDelta.Y = 0;
                _movementDelta.Z = 0;
            }

            DispatchEvent(new RoomObjectMoveEvent(RoomObjectMoveEvent.SLIDE_ANIMATION, Object));
        }

        LastUpdateTime = time;
    }

    protected virtual IVector3d? GetLocationOffset()
    {
        return null;
    }
}
