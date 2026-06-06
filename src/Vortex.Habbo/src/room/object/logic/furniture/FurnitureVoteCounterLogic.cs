namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureVoteCounterLogic (class_3486)
public class FurnitureVoteCounterLogic : FurnitureMultiStateLogic
{
    private const int UPDATE_INTERVAL = 33;
    private const int MAX_UPDATE_TIME = 1000;

    private int _targetTotal;
    private int _lastUpdateTime;
    private int _dynamicInterval;

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        _targetTotal = Object.GetState(0);
        int currentTotal = (int)Object.Model.GetNumber("furniture_vote_counter_count");

        if (currentTotal == _targetTotal)
        {
            return;
        }

        int diff = Math.Abs(_targetTotal - currentTotal);
        _dynamicInterval = diff > 0 ? Math.Max(UPDATE_INTERVAL, MAX_UPDATE_TIME / diff) : MAX_UPDATE_TIME;
        _lastUpdateTime = Environment.TickCount;
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (Object == null || _dynamicInterval <= 0)
        {
            return;
        }

        int currentTotal = (int)Object.Model.GetNumber("furniture_vote_counter_count");

        if (currentTotal == _targetTotal)
        {
            _dynamicInterval = 0;
            return;
        }

        int now = Environment.TickCount;

        if (now - _lastUpdateTime < _dynamicInterval)
        {
            return;
        }

        _lastUpdateTime = now;

        if (currentTotal < _targetTotal)
        {
            currentTotal++;
        }
        else
        {
            currentTotal--;
        }

        Object.ModelController.SetNumber("furniture_vote_counter_count", currentTotal);
    }
}
