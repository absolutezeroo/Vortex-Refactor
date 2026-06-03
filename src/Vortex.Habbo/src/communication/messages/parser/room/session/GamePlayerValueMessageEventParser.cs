using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.GamePlayerValueMessageEventParser
public class GamePlayerValueMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public int Value { get; private set; }

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        UserId = param1.ReadInteger();
        Value = param1.ReadInteger();
        return true;
    }
}
