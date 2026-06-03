using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

public class UniqueMachineIdEventParser : IMessageParser
{
    public string machineId { get; private set; } = string.Empty;

    public bool Flush()
    {
        machineId = string.Empty;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        machineId = param1.ReadString();
        return true;
    }
}
