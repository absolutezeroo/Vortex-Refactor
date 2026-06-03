using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.RoomQueueStatusMessageEventParser
public class RoomQueueStatusMessageEventParser : IMessageParser
{
    private readonly Dictionary<int, RoomQueueSet> _queueSets = new();

    public int FlatId { get; private set; }
    public int ActiveTarget { get; private set; }

    public bool Flush()
    {
        _queueSets.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        _queueSets.Clear();
        FlatId = param1.ReadInteger();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            string name = param1.ReadString();
            int target = param1.ReadInteger();
            if (i == 0)
            {
                ActiveTarget = target;
            }

            RoomQueueSet queueSet = new(name, target);
            int queueCount = param1.ReadInteger();
            for (int j = 0; j < queueCount; j++)
            {
                queueSet.AddQueue(param1.ReadString(), param1.ReadInteger());
            }
            _queueSets[target] = queueSet;
        }
        return true;
    }

    public List<int> GetQueueSetTargets()
    {
        return new List<int>(_queueSets.Keys);
    }
    public RoomQueueSet? GetQueueSet(int target)
    {
        return _queueSets.GetValueOrDefault(target);
    }
}

/// @see com.sulake.habbo.communication.messages.parser.room.session.class_1634
public class RoomQueueSet(string name, int target)
{
    private readonly List<(string Name, int Size)> _queues = [];

    public string Name => name;
    public int Target => target;

    public void AddQueue(string queueName, int size)
    {
        _queues.Add((queueName, size));
    }
    public int QueueCount => _queues.Count;
    public (string Name, int Size) GetQueue(int index)
    {
        return _queues[index];
    }
}
