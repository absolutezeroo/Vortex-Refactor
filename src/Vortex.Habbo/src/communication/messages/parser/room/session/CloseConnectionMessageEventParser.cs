using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.CloseConnectionMessageEventParser
public class CloseConnectionMessageEventParser : IMessageParser
{
    public bool Flush()
    {
        return true;
    }
    public bool Parse(IMessageDataWrapper param1)
    {
        return true;
    }
}
