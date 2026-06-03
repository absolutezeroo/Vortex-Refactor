using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectsMessageEventParser
public class ObjectsMessageEventParser : IMessageParser
{
    private readonly List<ObjectMessageData> _objects = [];

    public int ObjectCount => _objects.Count;

    public ObjectMessageData? GetObject(int index)
    {
        if (index < 0 || index >= _objects.Count)
        {
            return null;
        }
        ObjectMessageData data = _objects[index];
        data.SetReadOnly();
        return data;
    }

    public bool Flush()
    {
        _objects.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _objects.Clear();
        Dictionary<int, string> ownerMap = new();
        int ownerCount = param1.ReadInteger();
        for (int i = 0; i < ownerCount; i++)
        {
            int ownerId = param1.ReadInteger();
            string ownerName = param1.ReadString();
            ownerMap[ownerId] = ownerName;
        }
        int objectCount = param1.ReadInteger();
        for (int i = 0; i < objectCount; i++)
        {
            ObjectMessageData? data = ObjectDataParseHelper.ParseObjectData(param1);
            if (data != null)
            {
                ownerMap.TryGetValue(data.OwnerId, out string? ownerName);
                data.OwnerName = ownerName ?? "";
                _objects.Add(data);
            }
        }
        return true;
    }
}
