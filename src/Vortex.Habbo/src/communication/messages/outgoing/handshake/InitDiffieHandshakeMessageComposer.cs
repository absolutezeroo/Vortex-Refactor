using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class InitDiffieHandshakeMessageComposer : IMessageComposer, IPreEncryptionMessage
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return [];
    }
}
