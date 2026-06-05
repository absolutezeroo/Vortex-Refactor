using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Availability;

namespace Vortex.Habbo.Communication.Messages.Incoming.Availability;

/// @see com.sulake.habbo.communication.messages.incoming.availability.AvailabilityStatusMessageEvent
public class AvailabilityStatusMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(AvailabilityStatusMessageEventParser))
{
    private AvailabilityStatusMessageEventParser GetParser()
    {
        return (AvailabilityStatusMessageEventParser)parser!;
    }

    public bool isOpen => GetParser().isOpen;
    public bool onShutDown => GetParser().onShutDown;
    public bool isAuthenticHabbo => GetParser().isAuthenticHabbo;
}
