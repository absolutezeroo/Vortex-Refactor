using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.OpenConnectionMessageEventParser
public class OpenConnectionMessageEventParser : IMessageParser
{
    public int FlatId { get; private set; }

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        FlatId = param1.ReadInteger();
        return true;
    }
}
