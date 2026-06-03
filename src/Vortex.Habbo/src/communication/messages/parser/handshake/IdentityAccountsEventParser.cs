using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class IdentityAccountsEventParser : IMessageParser
{
    public Dictionary<int, string> accounts { get; private set; } = new();

    public bool Flush()
    {
        accounts = new Dictionary<int, string>();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        accounts = new Dictionary<int, string>();
        int count = param1.ReadInteger();
        for (int i = 0;
             i < count;
             i++)
        {
            accounts[param1.ReadInteger()] = param1.ReadString();
        }

        return true;
    }
}
