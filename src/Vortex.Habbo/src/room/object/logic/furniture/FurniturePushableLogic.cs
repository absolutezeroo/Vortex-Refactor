namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurniturePushableLogic
public class FurniturePushableLogic : FurnitureMultiStateLogic
{
    private const int ANIMATION_NOT_MOVING = 0;
    private const int ANIMATION_MOVING = 1;
    private const int MAX_ANIMATION_COUNT = 10;

    private Vector3d? _lastLocation;

    public override IRoomObjectController? Object
    {
        get => base.Object;
        set
        {
            base.Object = value;

            if (value == null)
            {
                return;
            }

            _lastLocation = new Vector3d();
            _lastLocation.Assign(value.Location);
        }
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        IRoomObjectModelController? model = Object?.ModelController;

        if (message is RoomObjectMoveUpdateMessage moveMsg && Object != null && model != null && _lastLocation != null)
        {
            int state = Object.GetState(0);
            int animationValue = GetAnimationValue(state);
            int intervalValue = GetUpdateIntervalValue(state);

            IVector3d location = Object.Location;
            double dx = location.X - _lastLocation.X;
            double dy = location.Y - _lastLocation.Y;

            if (dx != 0 || dy != 0)
            {
                if (animationValue < MAX_ANIMATION_COUNT)
                {
                    animationValue++;
                }
            }
            else
            {
                if (animationValue > 0)
                {
                    animationValue--;
                }
            }

            int newState = intervalValue * MAX_ANIMATION_COUNT + animationValue;

            Object.SetState(newState, 0);
            _lastLocation.Assign(location);
        }

        base.ProcessUpdateMessage(message);
    }

    private static int GetUpdateIntervalValue(int state)
    {
        return state / MAX_ANIMATION_COUNT;
    }

    private static int GetAnimationValue(int state)
    {
        return state % MAX_ANIMATION_COUNT;
    }
}
