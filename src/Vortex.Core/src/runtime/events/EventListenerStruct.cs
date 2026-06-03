// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as

using System;

namespace Vortex.Core.Runtime.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as — internal listener record
public sealed class EventListenerStruct
(
    Action<object?> callback,
    bool useCapture = false,
    int priority = 0,
    bool useWeakReference = false
)
{
    public Action<object?> Callback { get; } = callback;

    public bool UseCapture { get; } = useCapture;

    public int Priority { get; } = priority;

    public bool UseWeakReference { get; } = useWeakReference;
}
