using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ItemRemoveMessageEventParser
public class ItemRemoveMessageEventParser : IMessageParser
{
    public int ItemId { get; private set; }
    public int PickerId { get; private set; } = -1;

    public bool Flush() { ItemId = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        ItemId = int.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        PickerId = param1.ReadInteger();
        return true;
    }
}
