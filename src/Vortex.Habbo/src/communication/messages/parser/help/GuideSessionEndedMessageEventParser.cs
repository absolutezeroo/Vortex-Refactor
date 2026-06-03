using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Help;

/// @see com.sulake.habbo.communication.messages.parser.help.GuideSessionEndedMessageParser
public class GuideSessionEndedMessageEventParser : IMessageParser
{
    public int endReason { get; private set; }

    public bool Flush()
    {
        endReason = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        endReason = param1.ReadInteger();
        return true;
    }
}
