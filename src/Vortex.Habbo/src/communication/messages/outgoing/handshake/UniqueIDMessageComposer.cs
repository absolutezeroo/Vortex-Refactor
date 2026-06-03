using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class
    UniqueIdMessageComposer(string param1, string param2, string param3) : IMessageComposer
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return
        [
            param1,
            param2,
            param3,
        ];
    }
}
