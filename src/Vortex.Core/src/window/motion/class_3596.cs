// @see core/window/motion/class_3596.as

using System.Diagnostics;
using System.Linq;

namespace Vortex.Core.Window.Motion;

/// <summary>
/// Static motion manager. Central registry for all running motions.
/// Godot adaptation: uses Stopwatch for timing instead of Flash getTimer(),
/// and frame-based Update() instead of Flash Timer events.
/// </summary>
/// @see core/window/motion/class_3596.as
public static class MotionManager
{
    private static readonly List<Motion> _active = new();
    private static readonly List<Motion> _pendingAdd = new();
    private static readonly List<Motion> _pendingRemove = new();
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <summary>
    /// Returns milliseconds since application start (replaces Flash getTimer()).
    /// </summary>
    public static long GetTimer()
    {
        return _stopwatch.ElapsedMilliseconds;
    }

    /// @see class_3596.as::get isRunning
    public static bool IsRunning => _active.Count > 0 || _pendingAdd.Count > 0;

    /// @see class_3596.as::get isUpdating
    public static bool IsUpdating { get; private set; }

    /// @see class_3596.as::runMotion
    public static Motion RunMotion(Motion motion)
    {
        if (_active.Contains(motion) || _pendingAdd.Contains(motion))
        {
            return motion;
        }

        if (IsUpdating)
        {
            _pendingAdd.Add(motion);
        }
        else
        {
            _active.Add(motion);
            motion.Start();
        }

        return motion;
    }

    /// @see class_3596.as::removeMotion
    public static void RemoveMotion(Motion motion)
    {
        int index = _active.IndexOf(motion);

        if (index > -1)
        {
            if (IsUpdating)
            {
                if (!_pendingRemove.Contains(motion))
                {
                    _pendingRemove.Add(motion);
                }
            }
            else
            {
                _active.RemoveAt(index);
                if (motion.Running)
                {
                    motion.Stop();
                }
            }
        }
        else
        {
            index = _pendingAdd.IndexOf(motion);

            if (index > -1)
            {
                _pendingAdd.RemoveAt(index);
            }
        }
    }

    /// @see class_3596.as::getMotionByTag
    public static Motion? GetMotionByTag(string tag)
    {
        foreach (Motion motion in _active.Where(motion => motion.Tag == tag))
        {
            return motion;
        }

        return _pendingAdd.FirstOrDefault(motion => motion.Tag == tag);
    }

    /// @see class_3596.as::getMotionByTarget
    public static Motion? GetMotionByTarget(IWindow target)
    {
        foreach (Motion motion in _active.Where(motion => motion.Target == target))
        {
            return motion;
        }

        return _pendingAdd.FirstOrDefault(motion => motion.Target == target);
    }

    /// @see class_3596.as::getMotionByTagAndTarget
    public static Motion? GetMotionByTagAndTarget(string tag, IWindow target)
    {
        foreach (Motion motion in _active.Where(motion => motion.Tag == tag && motion.Target == target))
        {
            return motion;
        }

        return _pendingAdd.FirstOrDefault(motion => motion.Tag == tag && motion.Target == target);
    }

    /// @see class_3596.as::getNumRunningMotions
    public static int GetNumRunningMotions(IWindow target)
    {
        return _active.Count(motion => motion.Target == target);
    }

    /// <summary>
    /// Called each frame from the Godot _Process() callback.
    /// Replaces the AS3 Timer-driven onTick.
    /// </summary>
    /// @see class_3596.as::onTick
    public static void OnTick()
    {
        IsUpdating = true;
        long currentTimeMs = GetTimer();

        // (1) Flush pending additions — LIFO order (AS3 uses pop())
        for (int i = _pendingAdd.Count - 1; i >= 0; i--)
        {
            Motion motion = _pendingAdd[i];
            _active.Add(motion);
            motion.Start();
        }
        _pendingAdd.Clear();

        // (2) Flush pending removals from previous tick
        foreach (Motion motion in _pendingRemove)
        {
            _active.Remove(motion);
            if (motion.Running)
            {
                motion.Stop();
            }
        }
        _pendingRemove.Clear();

        // (3) Tick active motions forward (AS3 iterates forward)
        for (int i = 0; i < _active.Count; i++)
        {
            Motion motion = _active[i];

            if (motion.Running)
            {
                motion.Tick(currentTimeMs);

                if (motion.Complete)
                {
                    _pendingRemove.Add(motion);
                }
            }
            else
            {
                _pendingRemove.Add(motion);
            }
        }

        // (4) Flush post-tick removals
        foreach (Motion motion in _pendingRemove)
        {
            _active.Remove(motion);
            if (motion.Running)
            {
                motion.Stop();
            }
        }
        _pendingRemove.Clear();

        IsUpdating = false;
    }
}
