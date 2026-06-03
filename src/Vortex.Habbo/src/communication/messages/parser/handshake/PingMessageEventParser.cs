using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class PingMessageEventParser : IMessageParser
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
