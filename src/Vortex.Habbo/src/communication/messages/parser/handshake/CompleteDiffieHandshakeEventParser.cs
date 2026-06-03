using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class CompleteDiffieHandshakeEventParser : IMessageParser
{
    public string encryptedPublicKey { get; private set; } = string.Empty;

    public bool serverClientEncryption { get; private set; }

    public bool Flush()
    {
        encryptedPublicKey = string.Empty;
        serverClientEncryption = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        encryptedPublicKey = param1.ReadString();
        if (param1.bytesAvailable > 0)
        {
            serverClientEncryption = param1.ReadBoolean();
        }
        return true;
    }
}
