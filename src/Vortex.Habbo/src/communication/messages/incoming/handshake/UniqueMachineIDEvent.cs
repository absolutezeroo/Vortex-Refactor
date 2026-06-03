using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class UniqueMachineIdEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(UniqueMachineIdEventParser))
{
    public string machineId => GetParser().machineId;

    private UniqueMachineIdEventParser GetParser()
    {
        return (UniqueMachineIdEventParser)parser!;
    }
}
