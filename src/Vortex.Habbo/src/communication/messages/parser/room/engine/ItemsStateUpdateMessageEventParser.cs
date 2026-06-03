using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ItemsStateUpdateMessageEventParser
public class ItemsStateUpdateMessageEventParser : IMessageParser
{
    private readonly List<ItemStateUpdateMessageData> _items = [];

    public int ItemCount => _items.Count;

    public ItemStateUpdateMessageData? GetItemData(int index)
    {
        return index >= 0 && index < _items.Count ? _items[index] : null;
    }

    public bool Flush() { _items.Clear(); return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _items.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int id = param1.ReadInteger();
            string itemData = param1.ReadString();
            _items.Add(new ItemStateUpdateMessageData(id, itemData));
        }
        return true;
    }
}
