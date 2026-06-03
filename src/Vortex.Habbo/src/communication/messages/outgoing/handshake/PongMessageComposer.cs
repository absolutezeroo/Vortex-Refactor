using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class PongMessageComposer : IMessageComposer
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return [];
    }
}
