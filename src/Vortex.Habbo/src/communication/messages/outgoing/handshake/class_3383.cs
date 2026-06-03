using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Handshake;

public class Class3383(string param1, string param2, int param3) : IMessageComposer
{
    private readonly string _username = param1;

    private readonly int var_418 = param3;

    private readonly string var_937 = param2;

    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return [];
    }
}
