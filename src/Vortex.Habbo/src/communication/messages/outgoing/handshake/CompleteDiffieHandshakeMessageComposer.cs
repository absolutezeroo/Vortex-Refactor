using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class CompleteDiffieHandshakeMessageComposer(string param1) : IMessageComposer, IPreEncryptionMessage
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return
        [
            param1,
        ];
    }
}
