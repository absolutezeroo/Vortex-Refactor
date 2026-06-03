using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Availability;

public class LoginFailedHotelClosedMessageEventParser : IMessageParser
{
    public int openHour { get; private set; }

    public int openMinute { get; private set; }

    public bool Flush()
    {
        openHour = 0;
        openMinute = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        openHour = param1.ReadInteger();
        openMinute = param1.ReadInteger();
        return true;
    }
}
