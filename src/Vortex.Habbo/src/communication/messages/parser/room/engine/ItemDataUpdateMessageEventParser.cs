using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ItemDataUpdateMessageEventParser
public class ItemDataUpdateMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public string ItemData { get; private set; } = "";

    public bool Flush() { Id = 0; ItemData = ""; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = int.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        ItemData = param1.ReadString();
        return true;
    }
}
