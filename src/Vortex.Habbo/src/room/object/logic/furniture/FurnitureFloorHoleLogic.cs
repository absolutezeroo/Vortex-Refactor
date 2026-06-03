namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureFloorHoleLogic (class_3485)
public class FurnitureFloorHoleLogic : FurnitureMultiStateLogic
{
    private const int STATE_HOLE = 0;

    private int _currentState;
    private Vector3d? _lastLocation;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectFloorHoleEvent.ADD_HOLE,
            RoomObjectFloorHoleEvent.REMOVE_HOLE,
        ];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        if (Object != null && _currentState == STATE_HOLE)
        {
            DispatchEvent(new RoomObjectFloorHoleEvent(
                RoomObjectFloorHoleEvent.REMOVE_HOLE, Object));
        }
        base.Dispose();
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage message)
    {
        base.ProcessUpdateMessage(message);

        if (Object == null)
        {
            return;
        }

        if (message is RoomObjectDataUpdateMessage)
        {
            int state = Object.GetState(0);
            UpdateFloorHoleState(state);
        }

        if (message is RoomObjectMoveUpdateMessage || message.Location != null)
        {
            CheckLocationChange();
        }
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (Object == null)
        {
            return;
        }

        // Check automatic state index for floor hole updates
        double autoState = Object.Model.GetNumber("furniture_automatic_state_index");
        if (!double.IsNaN(autoState))
        {
            int state = (int)autoState % 2;
            UpdateFloorHoleState(state);
        }
    }

    private void UpdateFloorHoleState(int state)
    {
        if (Object == null || state == _currentState)
        {
            return;
        }

        if (state == STATE_HOLE)
        {
            DispatchEvent(new RoomObjectFloorHoleEvent(
                RoomObjectFloorHoleEvent.ADD_HOLE, Object));
        }
        else
        {
            DispatchEvent(new RoomObjectFloorHoleEvent(
                RoomObjectFloorHoleEvent.REMOVE_HOLE, Object));
        }

        _currentState = state;
    }

    private void CheckLocationChange()
    {
        if (Object == null)
        {
            return;
        }

        IVector3d loc = Object.Location;

        if (_lastLocation == null)
        {
            _lastLocation = new Vector3d();
            _lastLocation.Assign(loc);

            if (_currentState == STATE_HOLE)
            {
                DispatchEvent(new RoomObjectFloorHoleEvent(
                    RoomObjectFloorHoleEvent.ADD_HOLE, Object));
            }
            return;
        }

        if (_lastLocation.X != loc.X || _lastLocation.Y != loc.Y)
        {
            if (_currentState == STATE_HOLE)
            {
                DispatchEvent(new RoomObjectFloorHoleEvent(
                    RoomObjectFloorHoleEvent.REMOVE_HOLE, Object));
                _lastLocation.Assign(loc);
                DispatchEvent(new RoomObjectFloorHoleEvent(
                    RoomObjectFloorHoleEvent.ADD_HOLE, Object));
            }
            else
            {
                _lastLocation.Assign(loc);
            }
        }
    }
}
