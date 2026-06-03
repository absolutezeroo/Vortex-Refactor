using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class IdentityAccountsEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(IdentityAccountsEventParser))
{
    public IdentityAccountsEventParser GetParser()
    {
        return (IdentityAccountsEventParser)parser!;
    }
}
