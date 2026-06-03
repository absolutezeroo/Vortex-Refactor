using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class InitDiffieHandshakeEventParser : IMessageParser
{
    public string encryptedPrime { get; private set; } = string.Empty;

    public string encryptedGenerator { get; private set; } = string.Empty;

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        encryptedPrime = param1.ReadString();
        encryptedGenerator = param1.ReadString();
        return true;
    }
}
