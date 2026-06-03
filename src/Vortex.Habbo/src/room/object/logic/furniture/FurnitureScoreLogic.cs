namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureScoreLogic (class_3509)
public class FurnitureScoreLogic : FurnitureLogic
{
    private const int UPDATE_INTERVAL = 50;
    private const int MAX_UPDATE_TIME = 3000;

    private int _targetScore;
    private int _lastUpdateTime;
    private int _dynamicInterval;

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        _targetScore = Object.GetState(0);

        int currentScore = (int)Object.Model.GetNumber("furniture_score_count");

        if (currentScore == _targetScore)
        {
            return;
        }

        int diff = Math.Abs(_targetScore - currentScore);
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

        int currentScore = (int)Object.Model.GetNumber("furniture_score_count");

        if (currentScore == _targetScore)
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

        if (currentScore < _targetScore)
        {
            currentScore++;
        }
        else
        {
            currentScore--;
        }

        Object.ModelController.SetNumber("furniture_score_count", currentScore);
    }
}
