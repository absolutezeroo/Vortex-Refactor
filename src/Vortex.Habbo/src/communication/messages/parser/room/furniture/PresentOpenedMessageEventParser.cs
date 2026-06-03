using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.parser.room.furniture.PresentOpenedMessageParser
public class PresentOpenedMessageEventParser : IMessageParser
{
    public string ItemType { get; private set; } = "";
    public int ClassId { get; private set; }
    public string ProductCode { get; private set; } = "";
    public int PlacedItemId { get; private set; }
    public string PlacedItemType { get; private set; } = "";
    public bool PlacedInRoom { get; private set; }
    public string PetFigureString { get; private set; } = "";

    public bool Flush()
    {
        ItemType = "";
        ClassId = 0;
        ProductCode = "";
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        ItemType = param1.ReadString();
        ClassId = param1.ReadInteger();
        ProductCode = param1.ReadString();
        PlacedItemId = param1.ReadInteger();
        PlacedItemType = param1.ReadString();
        PlacedInRoom = param1.ReadBoolean();
        PetFigureString = param1.ReadString();
        return true;
    }
}
