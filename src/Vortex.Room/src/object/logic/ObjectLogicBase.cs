using System;
using System.Reflection;
using System.Xml.Linq;

using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Utils;

namespace Vortex.Room.Object.Logic;

/// @see com.sulake.room.object.logic.ObjectLogicBase
public class ObjectLogicBase : IRoomObjectEventHandler
{
    private object? _eventDispatcher;
    private IRoomObjectController? _object;

    public object? EventDispatcher
    {
        get => _eventDispatcher;
        set
        {
            _eventDispatcher = value;
            _dispatchDelegate = null;
        }
    }

    public virtual IRoomObjectController? Object
    {
        get => _object;
        set
        {
            if (_object == value)
            {
                return;
            }
            _object?.SetEventHandler(null!);
            if (value == null)
            {
                Dispose();
                _object = null;
            }
            else
            {
                _object = value;
                _object.SetEventHandler(this);
            }
        }
    }

    public virtual string[]? GetEventTypes()
    {
        return [];
    }

    protected static string[] GetAllEventTypes(string[] baseTypes, string[] additionalTypes)
    {
        List<string> result = new(baseTypes);
        foreach (string type in additionalTypes)
        {
            if (!result.Contains(type))
            {
                result.Add(type);
            }
        }
        return result.ToArray();
    }

    public virtual void Dispose()
    {
        _object = null;
    }

    public virtual void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
    }

    public virtual void Initialize(XElement? xml)
    {
    }

    public virtual void Update(int time)
    {
    }

    public virtual void ProcessUpdateMessage(RoomObjectUpdateMessage message)
    {
        if (message != null && _object != null)
        {
            if (message.Location != null)
            {
                _object.SetLocation(message.Location);
            }
            if (message.Direction != null)
            {
                _object.SetDirection(message.Direction);
            }
        }
    }

    public virtual void UseObject()
    {
    }

    /// <summary>
    /// Dispatches a room object event via the event dispatcher.
    /// The dispatcher is typed as object? for layer decoupling —
    /// at runtime it implements DispatchEvent(object?).
    /// </summary>
    protected void DispatchEvent(RoomObjectEvent evt)
    {
        if (_eventDispatcher == null)
        {
            return;
        }

        // Use cached delegate for performance; fallback to reflection on first call.
        _dispatchDelegate ??= CreateDispatchDelegate(_eventDispatcher);
        _dispatchDelegate.Invoke(evt);
    }

    private Action<object?>? _dispatchDelegate;

    private static Action<object?> CreateDispatchDelegate(object dispatcher)
    {
        MethodInfo? method = dispatcher.GetType().GetMethod("DispatchEvent", [typeof(object)]);
        if (method != null)
        {
            return (evt) => method.Invoke(dispatcher, [evt]);
        }
        return (_) => { };
    }

    public virtual void TearDown()
    {
    }

    public virtual string? Widget => null;

    public virtual string? ContextMenu => null;
}
