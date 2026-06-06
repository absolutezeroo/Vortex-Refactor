namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureIceStormLogic (class_3475)
public class FurnitureIceStormLogic : FurnitureLogic
{
    private int _nextState;
    private double _nextExtra;
    private int _deferredTime;

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        if (message is RoomObjectDataUpdateMessage dataMsg && Object != null)
        {
            int rawState = dataMsg.State;
            int stateValue = rawState / 1000;
            int delayMs = rawState % 1000;

            if (delayMs == 0)
            {
                ApplyState(stateValue, dataMsg.Extra);
            }
            else
            {
                _nextState = stateValue;
                _nextExtra = dataMsg.Extra;
                _deferredTime = Environment.TickCount + delayMs;
            }
            return;
        }

        base.ProcessUpdateMessage(message);
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (_deferredTime <= 0 || Environment.TickCount < _deferredTime)
        {
            return;
        }

        ApplyState(_nextState, _nextExtra);

        _deferredTime = 0;
    }

    private void ApplyState(int state, double extra)
    {
        if (Object == null)
        {
            return;
        }

        Object.SetState(state, 0);

        if (!double.IsNaN(extra))
        {
            Object.ModelController.SetNumber("furniture_extras", extra);
        }

        Object.ModelController.SetNumber("furniture_state_update_time", LastUpdateTime);
    }
}
