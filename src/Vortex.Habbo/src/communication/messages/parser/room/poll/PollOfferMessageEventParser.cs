// @see com.sulake.habbo.communication.messages.parser.room.poll.PollOfferParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Poll;

/// @see com.sulake.habbo.communication.messages.parser.room.poll.PollOfferParser
public class PollOfferMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public string? Headline { get; private set; }
    public string? Summary { get; private set; }

    public bool Flush()
    {
        Id = 0;
        Headline = null;
        Summary = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        Id = param1.ReadInteger();
        Headline = param1.ReadString();
        Summary = param1.ReadString();
        return true;
    }
}
