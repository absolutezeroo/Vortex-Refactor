using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Availability;

namespace Vortex.Habbo.Communication.Messages.Incoming.Availability;

public class MaintenanceStatusMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(MaintenanceStatusMessageEventParser))
{
    public MaintenanceStatusMessageEventParser GetParser()
    {
        return (MaintenanceStatusMessageEventParser)parser!;
    }
}
