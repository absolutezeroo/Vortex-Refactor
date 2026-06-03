// @see core/window/utils/GenericEventQueue.as

using System;

using Godot;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Base event queue with index-based iteration over buffered input events.
/// </summary>
/// @see core/window/utils/GenericEventQueue.as
public class GenericEventQueue : IEventQueue, IDisposable
{
    protected bool _disposed;
    protected readonly List<InputEvent> _eventArray = new();
    protected uint _index;
    protected bool _ended = true;

    public GenericEventQueue() { }

    /// @see GenericEventQueue.as::get length
    public virtual uint Length => (uint)_eventArray.Count;

    public uint EventCount => (uint)_eventArray.Count;

    public bool Disposed => _disposed;

    /// @see GenericEventQueue.as::dispose
    public virtual void DisposeQueue()
    {
        if (_disposed)
        {
            return;
        }

        _eventArray.Clear();
        _disposed = true;
    }

    void IDisposable.Dispose()
    {
        DisposeQueue();
        GC.SuppressFinalize(this);
    }

    /// @see GenericEventQueue.as::begin
    public virtual void Begin()
    {
        if (!_ended)
        {
            Flush();
        }

        _index = 0;
        _ended = false;
    }

    /// @see GenericEventQueue.as::next
    public virtual object? Next()
    {
        if (_index >= _eventArray.Count)
        {
            return null;
        }

        InputEvent evt = _eventArray[(int)_index];
        _index++;

        return evt;
    }

    /// @see GenericEventQueue.as::remove
    public virtual void Remove()
    {
        _eventArray.RemoveAt((int)(_index - 1));

        if (_index > 0)
        {
            _index--;
        }
    }

    /// @see GenericEventQueue.as::end
    public virtual void End()
    {
        _index = 0;
        _ended = true;
    }

    /// @see GenericEventQueue.as::flush
    public virtual void Flush()
    {
        _eventArray.Clear();
        _index = 0;
    }

    /// @see GenericEventQueue.as::eventListener
    public virtual object? EventListener(params object?[] args)
    {
        return null;
    }

    /// <summary>
    /// Enqueue a Godot InputEvent for processing.
    /// </summary>
    public void Enqueue(InputEvent evt)
    {
        _eventArray.Add(evt);
    }
}
