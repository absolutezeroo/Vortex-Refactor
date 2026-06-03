using System;
using System.Linq;

namespace Vortex.Core.Communication.Messages;

/// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as
public class MessageClassManager : IDisposable
{
    private readonly Dictionary<int, List<IMessageEvent>> _messageIdByEventClass = new();
    private readonly Dictionary<Type, int> _messageInstancesById = new();
    private readonly Dictionary<Type, int> _messageIdByComposerClass = new();

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::dispose
    public void Dispose()
    {
        foreach (IMessageEvent e in _messageIdByEventClass.SelectMany(pair => pair.Value))
        {
            e.Dispose();
        }

        _messageIdByEventClass.Clear();

        GC.SuppressFinalize(this);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::registerMessages
    public void RegisterMessages(IMessageConfiguration param1)
    {
        foreach (int key in param1.events.Keys)
        {
            RegisterMessageEventClass(key, param1.events[key]);
        }

        foreach (int key in param1.composers.Keys)
        {
            RegisterMessageComposerClass(key, param1.composers[key]);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::registerMessageComposerClass
    private void RegisterMessageComposerClass(int param1, Type param2)
    {
        if (!_messageIdByComposerClass.TryAdd(param2, param1))
        {
            throw new Exception("Duplicate message ID definition for composer class " + param2.FullName);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::registerMessageEventClass
    private void RegisterMessageEventClass(int param1, Type param2)
    {
        if (!_messageInstancesById.TryAdd(param2, param1))
        {
            throw new Exception("Duplicate message ID definition for event class " + param2.FullName);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::registerMessageEvent
    public void RegisterMessageEvent(IMessageEvent param1)
    {
        Type eventType = param1.GetType();

        if (!_messageInstancesById.TryGetValue(eventType, out int id))
        {
            throw new Exception("Unknown message event class " + eventType.FullName);
        }

        if (!_messageIdByEventClass.TryGetValue(id, out List<IMessageEvent>? list))
        {
            list = [];
            _messageIdByEventClass[id] = list;

            param1.parser = (IMessageParser?)Activator.CreateInstance(param1.parserClass);
        }
        else if (list.Count > 0)
        {
            param1.parser = list[0].parser;
        }

        list.Add(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::unregisterMessageEvent
    public void UnregisterMessageEvent(IMessageEvent param1)
    {
        Type eventType = param1.GetType();

        if (!_messageInstancesById.TryGetValue(eventType, out int id))
        {
            return;
        }

        if (!_messageIdByEventClass.TryGetValue(id, out List<IMessageEvent>? list))
        {
            return;
        }

        list.Remove(param1);

        if (list.Count == 0)
        {
            _messageIdByEventClass.Remove(id);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::getMessageIDForComposer
    public int GetMessageIdForComposer(IMessageComposer param1)
    {
        return _messageIdByComposerClass.GetValueOrDefault(param1.GetType(), -1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/communication/messages/class_3631.as::getMessageEventsForID
    public List<IMessageEvent> GetMessageEventsForId(int param1)
    {
        List<IMessageEvent> result = new();

        if (!_messageIdByEventClass.TryGetValue(param1, out List<IMessageEvent>? list))
        {
            return result;
        }

        result.AddRange(list);

        return result;
    }
}
