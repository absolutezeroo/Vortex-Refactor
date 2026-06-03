using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Availability;

namespace Vortex.Habbo.Communication.Messages.Incoming.Availability;

public class LoginFailedHotelClosedMessageEvent(Action<IMessageEvent> param1)
    : MessageEvent(param1, typeof(LoginFailedHotelClosedMessageEventParser))
{
    public LoginFailedHotelClosedMessageEventParser GetParser()
    {
        return (LoginFailedHotelClosedMessageEventParser)parser!;
    }
}
