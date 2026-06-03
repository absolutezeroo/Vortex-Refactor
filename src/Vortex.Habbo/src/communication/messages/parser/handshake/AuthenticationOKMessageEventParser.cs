using Vortex.Core.Communication.Messages;

using Array = Godot.Collections.Array;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class AuthenticationOkMessageEventParser : IMessageParser
{
    public int accountId { get; private set; } = -1;

    public Array suggestedLoginActions { get; private set; } = new();

    public int identityId { get; private set; } = -1;

    public bool Flush()
    {
        accountId = -1;
        suggestedLoginActions = new Array();
        identityId = -1;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        accountId = param1.ReadInteger();
        int count = param1.ReadInteger();

        for (int i = 0;
             i < count;
             i++)
        {
            suggestedLoginActions.Add(param1.ReadShort());
        }

        identityId = param1.ReadInteger();

        return true;
    }
}
