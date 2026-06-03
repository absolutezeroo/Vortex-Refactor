using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ItemsMessageEventParser
public class ItemsMessageEventParser : IMessageParser
{
    private readonly List<ItemMessageData> _items = [];

    public int ItemCount => _items.Count;

    public ItemMessageData? GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            return null;
        }
        ItemMessageData data = _items[index];
        data.SetReadOnly();
        return data;
    }

    public bool Flush()
    {
        _items.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        _items.Clear();
        Dictionary<int, string> ownerMap = new();
        int ownerCount = param1.ReadInteger();
        for (int i = 0; i < ownerCount; i++)
        {
            int ownerId = param1.ReadInteger();
            string ownerName = param1.ReadString();
            ownerMap[ownerId] = ownerName;
        }
        int itemCount = param1.ReadInteger();
        for (int i = 0; i < itemCount; i++)
        {
            ItemMessageData data = ItemDataParseHelper.ParseItemData(param1);
            ownerMap.TryGetValue(data.OwnerId, out string? ownerName);
            data.OwnerName = ownerName ?? "";
            _items.Add(data);
        }
        return true;
    }
}
