using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Availability;

/// @see com.sulake.habbo.communication.messages.parser.availability.AvailabilityStatusMessageParser
public class AvailabilityStatusMessageEventParser : IMessageParser
{
    public bool isOpen { get; private set; }
    public bool onShutDown { get; private set; }
    public bool isAuthenticHabbo { get; private set; }

    public bool Flush()
    {
        isOpen = false;
        onShutDown = false;
        isAuthenticHabbo = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        isOpen = param1.ReadBoolean();
        onShutDown = param1.ReadBoolean();
        if (param1.bytesAvailable > 0)
        {
            isAuthenticHabbo = param1.ReadBoolean();
        }
        return true;
    }
}
