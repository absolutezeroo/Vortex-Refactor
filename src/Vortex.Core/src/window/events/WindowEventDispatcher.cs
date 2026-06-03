// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/events/WindowEventDispatcher.as

using System;
using System.Linq;

namespace Vortex.Core.Window.Events;

/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/events/WindowEventDispatcher.as
public class WindowEventDispatcher : IDisposable
{
    private readonly IWindow? _owner;
    private readonly Dictionary<string, List<ListenerEntry>> _listeners = new(StringComparer.Ordinal);

    /// @see WindowEventDispatcher.as::WindowEventDispatcher
    public WindowEventDispatcher(IWindow? owner = null)
    {
        _owner = owner;
    }

    /// @see WindowEventDispatcher.as::get disposed
    public bool disposed { get; private set; }

    /// @see WindowEventDispatcher.as::addEventListener
    public void AddEventListener(string type, Action<WindowEvent, IWindow> handler, int priority = 0)
    {
        if (disposed || string.IsNullOrEmpty(type) || handler == null)
        {
            return;
        }

        if (!_listeners.TryGetValue(type, out List<ListenerEntry>? list))
        {
            list = [];
            _listeners[type] = list;
        }

        if (list.Any(t => t.Handler == handler))
        {
            return;
        }

        list.Add(new ListenerEntry(handler, priority));

        if (list.Count > 1)
        {
            list.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }
    }

    /// @see WindowEventDispatcher.as::removeEventListener
    public void RemoveEventListener(string type, Action<WindowEvent, IWindow> handler)
    {
        if (disposed || string.IsNullOrEmpty(type) || handler == null)
        {
            return;
        }

        if (!_listeners.TryGetValue(type, out List<ListenerEntry>? list))
        {
            return;
        }

        for (int i = list.Count - 1;
             i >= 0;
             i--)
        {
            if (list[i].Handler != handler)
            {
                continue;
            }

            list.RemoveAt(i);

            break;
        }

        if (list.Count == 0)
        {
            _listeners.Remove(type);
        }
    }

    /// @see WindowEventDispatcher.as::dispatchEvent
    public bool DispatchEvent(WindowEvent? evt)
    {
        if (disposed || evt == null)
        {
            return false;
        }

        if (!_listeners.TryGetValue(evt.type, out List<ListenerEntry>? list) || list.Count == 0)
        {
            return false;
        }

        IWindow? target = _owner ?? evt.window;
        ListenerEntry[] snapshot = list.ToArray();

        foreach (ListenerEntry entry in snapshot)
        {
            if (evt.IsDefaultPrevented())
            {
                break;
            }

            entry.Handler(evt, target!);
        }

        // AS3 returns true when event was NOT prevented (dispatched cleanly)
        return !evt.IsDefaultPrevented();
    }

    /// @see WindowEventDispatcher.as::hasEventListener
    public bool HasEventListener(string type)
    {
        if (disposed || string.IsNullOrEmpty(type))
        {
            return false;
        }

        return _listeners.TryGetValue(type, out List<ListenerEntry>? list) && list.Count > 0;
    }

    /// @see WindowEventDispatcher.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        _listeners.Clear();
    }

    private readonly struct ListenerEntry(Action<WindowEvent, IWindow> handler, int priority)
    {
        public Action<WindowEvent, IWindow> Handler { get; } = handler;
        public int Priority { get; } = priority;
    }
}
