// @see WIN63-202407091256-704579380-Source-main/core/utils/class_1595.as

using System;

namespace Vortex.Core.Runtime;

/// <summary>
/// Periodic memory purge trigger. .NET adaptation of AS3's System.gc() / pauseForGCIfCollectionImminent.
/// In .NET, GC is automatic; this becomes a lightweight periodic GC.Collect hint.
/// </summary>
/// @see core/utils/class_1595.as
public static class PurgeTrigger
{
    private static ulong _lastTriggerTime;

    /// @see class_1595.as::get frequencyMilliSeconds
    public static uint FrequencyMilliseconds { get; set; } = 60000;

    /// @see class_1595.as::get isRunning
    public static bool IsRunning { get; private set; }

    /// @see class_1595.as::start
    public static void Start()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        _lastTriggerTime = (ulong)Environment.TickCount64;
    }

    /// @see class_1595.as::stop
    public static void Stop()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Should be called periodically (e.g., from frame update) to check if a GC hint is due.
    /// </summary>
    public static void CheckAndTrigger()
    {
        if (!IsRunning)
        {
            return;
        }

        ulong now = (ulong)Environment.TickCount64;

        if (now - _lastTriggerTime < FrequencyMilliseconds)
        {
            return;
        }

        _lastTriggerTime = now;

        Trigger();
    }

    /// @see class_1595.as::trigger
    public static void Trigger()
    {
        // .NET adaptation: Request a non-blocking, optimized GC of gen 0 only.
        // This is much lighter than AS3's System.gc() which did a full collection.
        GC.Collect(0, GCCollectionMode.Optimized, false);
    }
}
