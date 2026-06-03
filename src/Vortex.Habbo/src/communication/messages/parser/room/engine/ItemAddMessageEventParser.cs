using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ItemAddMessageEventParser
public class ItemAddMessageEventParser : IMessageParser
{
    private ItemMessageData? _data;

    public ItemMessageData? Data
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
        _data = ItemDataParseHelper.ParseItemData(param1);
        _data.OwnerName = param1.ReadString();
        return true;
    }
}
