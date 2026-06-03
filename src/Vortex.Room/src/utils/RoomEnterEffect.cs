using System;

namespace Vortex.Room.Utils;

/// <summary>
/// Timer-based room entry fade effect. All static — singleton per process.
/// </summary>
/// @see com.sulake.room.utils.RoomEnterEffect
public static class RoomEnterEffect
{
    public const int STATE_NOT_INITIALIZED = 0;
    public const int STATE_START_DELAY = 1;
    public const int STATE_RUNNING = 2;
    public const int STATE_OVER = 3;

    private static int _state;
    private static bool _visualizationOn;
    private static double _delta;
    private static int _startTime;
    private static int _startDelay = 20000;
    private static int _runningDuration = 2000;

    public static void Init(int startDelay, int duration)
    {
        _delta = 0;
        _startDelay = startDelay;
        _runningDuration = duration;
        _startTime = Environment.TickCount;
        _state = STATE_START_DELAY;
    }

    public static void TurnVisualizationOn()
    {
        if (_state is STATE_NOT_INITIALIZED or STATE_OVER)
        {
            return;
        }

        int elapsed = Environment.TickCount - _startTime;

        if (elapsed > _startDelay + _runningDuration)
        {
            _state = STATE_OVER;

            return;
        }

        _visualizationOn = true;

        if (elapsed < _startDelay)
        {
            _state = STATE_START_DELAY;

            return;
        }

        _state = STATE_RUNNING;
        _delta = (double)(elapsed - _startDelay) / _runningDuration;
    }

    public static void TurnVisualizationOff()
    {
        _visualizationOn = false;
    }

    public static bool IsVisualizationOn()
    {
        return _visualizationOn && IsRunning();
    }

    public static bool IsRunning()
    {
        return _state is STATE_START_DELAY or STATE_RUNNING;
    }

    public static double GetDelta(double min = 0, double max = 1)
    {
        return Math.Min(Math.Max(_delta, min), max);
    }

    public static int TotalRunningTime => _startDelay + _runningDuration;
}
