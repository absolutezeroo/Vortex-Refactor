using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Help;

/// @see com.sulake.habbo.communication.messages.parser.help.GuideSessionErrorMessageParser
public class GuideSessionErrorMessageEventParser : IMessageParser
{
    public int errorCode { get; private set; }

    public bool Flush()
    {
        errorCode = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        errorCode = param1.ReadInteger();
        return true;
    }
}
