using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ItemStateUpdateMessageEventParser
public class ItemStateUpdateMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public string ItemData { get; private set; } = "";
    public int State { get; private set; }

    public bool Flush() { Id = 0; ItemData = ""; State = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = param1.ReadInteger();
        ItemData = param1.ReadString();
        State = 0;
        if (double.TryParse(ItemData, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            State = int.Parse(ItemData, CultureInfo.InvariantCulture);
        }
        return true;
    }
}
