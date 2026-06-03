using Godot;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class SsoTicketMessageComposer(string param1) : IMessageComposer
{
    private readonly List<object> var_368 =
    [
        param1,
        (int)(Time.GetTicksMsec() & 0x7FFFFFFF),
    ];

    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return var_368;
    }
}
