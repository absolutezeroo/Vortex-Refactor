using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class GenericErrorEventParser : IMessageParser
{
    public int errorCode { get; private set; }

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        errorCode = param1.ReadInteger();
        return true;
    }
}
