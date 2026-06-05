using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

/// @see com.sulake.habbo.communication.messages.parser.handshake.NoobnessLevelMessageEventMessageParser
public class NoobnessLevelMessageEventParser : IMessageParser
{
    public int noobnessLevel { get; private set; }

    public bool Flush()
    {
        noobnessLevel = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        noobnessLevel = param1.ReadInteger();
        return true;
    }
}
