using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class DisconnectReasonEventParser : IMessageParser
{
    public int reason { get; private set; } = -1;

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1.bytesAvailable > 0)
        {
            reason = param1.ReadInteger();
        }
        return true;
    }
}
