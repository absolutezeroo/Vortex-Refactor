using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Availability;

public class MaintenanceStatusMessageEventParser : IMessageParser
{
    public bool isInMaintenance { get; private set; }

    public int minutesUntilMaintenance { get; private set; }

    public int duration { get; private set; } = 15;

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        isInMaintenance = param1.ReadBoolean();
        minutesUntilMaintenance = param1.ReadInteger();
        if (param1.bytesAvailable > 0)
        {
            duration = param1.ReadInteger();
        }
        return true;
    }
}
