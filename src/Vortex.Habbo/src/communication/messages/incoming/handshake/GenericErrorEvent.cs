using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class GenericErrorEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(GenericErrorEventParser))
{
    public GenericErrorEventParser GetParser()
    {
        return (GenericErrorEventParser)parser!;
    }
}
