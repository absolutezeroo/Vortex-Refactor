using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.YouAreNotSpectatorMessageEventParser
public class YouAreNotSpectatorMessageEventParser : IMessageParser
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
