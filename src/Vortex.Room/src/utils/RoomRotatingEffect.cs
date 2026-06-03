using System;
using System.Timers;

namespace Vortex.Room.Utils;

/// <summary>
/// Timer-based room rotation effect. All static — singleton per process.
/// Uses System.Timers.Timer instead of Flash Timer.
/// </summary>
/// @see com.sulake.room.utils.RoomRotatingEffect
public static class RoomRotatingEffect
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
    private static int _runningDuration = 5000;
    private static Timer? _timer;

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

        if (_timer == null)
        {
            _timer = new Timer(_runningDuration)
            {
                AutoReset = false,
            };
            _timer.Elapsed += OnTimerComplete;
            _timer.Start();
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

    private static void OnTimerComplete(object? sender, ElapsedEventArgs e)
    {
        _visualizationOn = false;

        if (_timer == null)
        {
            return;
        }

        _timer.Stop();
        _timer.Elapsed -= OnTimerComplete;
        _timer.Dispose();
        _timer = null;
    }

    public static bool IsVisualizationOn()
    {
        return _visualizationOn && IsRunning();
    }

    private static bool IsRunning()
    {
        return _state is STATE_START_DELAY or STATE_RUNNING;
    }
}
