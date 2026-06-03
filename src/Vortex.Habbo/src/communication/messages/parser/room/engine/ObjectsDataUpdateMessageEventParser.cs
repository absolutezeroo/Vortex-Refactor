using System.Globalization;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Room;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectsDataUpdateMessageEventParser
public class ObjectsDataUpdateMessageEventParser : IMessageParser
{
    private readonly List<ObjectDataUpdateMessageData> _objects = [];

    public int ObjectCount => _objects.Count;

    public ObjectDataUpdateMessageData? GetObjectData(int index)
    {
        return index >= 0 && index < _objects.Count ? _objects[index] : null;
    }

    public bool Flush() { _objects.Clear(); return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _objects.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int id = param1.ReadInteger();
            IStuffData data = ObjectDataParseHelper.ParseStuffData(param1);
            int state = 0;
            if (double.TryParse(data.GetLegacyString(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                state = int.Parse(data.GetLegacyString(), CultureInfo.InvariantCulture);
            }
            _objects.Add(new ObjectDataUpdateMessageData(id, state, data));
        }
        return true;
    }
}
