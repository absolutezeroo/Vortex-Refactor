using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectAddMessageEventParser
public class ObjectAddMessageEventParser : IMessageParser
{
    private ObjectMessageData? _data;

    public ObjectMessageData? Data
    {
        get { _data?.SetReadOnly(); return _data; }
    }

    public bool Flush() { _data = null; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _data = ObjectDataParseHelper.ParseObjectData(param1);
        if (_data != null)
        {
            _data.OwnerName = param1.ReadString();
        }
        return true;
    }
}
