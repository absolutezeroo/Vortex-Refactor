using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.parser.room.furniture.DiceValueMessageParser
public class DiceValueMessageEventParser : IMessageParser
{
    public int Id { get; private set; } = -1;
    public int Value { get; private set; }

    public bool Flush() { Id = -1; Value = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = param1.ReadInteger();
        Value = param1.ReadInteger();
        return true;
    }
}
