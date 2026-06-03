// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as

using System;
using System.Linq;
using System.Reflection;

namespace Vortex.Core.Runtime.Events;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as
public sealed class EventDispatcherWrapper : IDisposable
{
    private readonly Dictionary<string, List<EventListenerStruct>> _listeners = new(StringComparer.Ordinal);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::get disposed
    public bool disposed { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::addEventListener
    public void AddEventListener(string param1, Action<object?> param2, bool useCapture = false, int priority = 0,
        bool useWeakReference = false)
    {
        if (disposed)
        {
            return;
        }

        if (!_listeners.TryGetValue(param1, out List<EventListenerStruct>? handlers))
        {
            handlers = [];
            _listeners[param1] = handlers;
        }

        // Don't add duplicate (same callback + same useCapture)
        if (handlers.Any(t => t.Callback == param2 && t.UseCapture == useCapture))
        {
            return;
        }

        EventListenerStruct entry = new(param2, useCapture, priority, useWeakReference);

        // Insert by priority: higher priority = earlier in list
        int insertIndex = handlers.Count;

        for (int i = 0;
             i < handlers.Count;
             i++)
        {
            if (handlers[i].Priority >= priority)
            {
                continue;
            }

            insertIndex = i;

            break;
        }

        handlers.Insert(insertIndex, entry);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::removeEventListener
    public void RemoveEventListener(string param1, Action<object?> param2, bool useCapture = false)
    {
        if (disposed)
        {
            return;
        }

        if (!_listeners.TryGetValue(param1, out List<EventListenerStruct>? handlers))
        {
            return;
        }

        for (int i = handlers.Count - 1;
             i >= 0;
             i--)
        {
            if (handlers[i].Callback != param2 || handlers[i].UseCapture != useCapture)
            {
                continue;
            }

            handlers.RemoveAt(i);

            break;
        }

        if (handlers.Count == 0)
        {
            _listeners.Remove(param1);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::dispatchEvent
    public bool DispatchEvent(string param1, object? param2 = null)
    {
        if (disposed)
        {
            return false;
        }

        if (!_listeners.TryGetValue(param1, out List<EventListenerStruct>? handlers))
        {
            return true;
        }

        foreach (EventListenerStruct entry in handlers.ToList())
        {
            entry.Callback(param2);
        }

        return true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::dispatchEvent
    public bool DispatchEvent(object? param1)
    {
        if (disposed || param1 == null)
        {
            return false;
        }

        string? eventType = null;

        if (param1 is LockEvent lockEvent)
        {
            eventType = lockEvent.type;
        }
        else
        {
            PropertyInfo? typeProperty = param1.GetType().GetProperty("type");

            if (typeProperty?.PropertyType == typeof(string))
            {
                eventType = typeProperty.GetValue(param1) as string;
            }
        }

        return !string.IsNullOrEmpty(eventType) && DispatchEvent(eventType, param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::hasEventListener
    public bool HasEventListener(string param1)
    {
        return !disposed && _listeners.ContainsKey(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::callEventListeners
    public void CallEventListeners(string param1)
    {
        if (disposed || !_listeners.TryGetValue(param1, out List<EventListenerStruct>? handlers))
        {
            return;
        }

        foreach (EventListenerStruct entry in handlers.ToList())
        {
            entry.Callback(null);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/events/EventDispatcherWrapper.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        _listeners.Clear();
        disposed = true;
    }
}
