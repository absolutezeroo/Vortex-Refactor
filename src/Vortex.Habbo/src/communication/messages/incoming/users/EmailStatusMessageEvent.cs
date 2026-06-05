using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Users;

namespace Vortex.Habbo.Communication.Messages.Incoming.Users;

/// @see com.sulake.habbo.communication.messages.incoming.users.class_547
public class EmailStatusMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(EmailStatusMessageEventParser))
{
    public string email => ((EmailStatusMessageEventParser)parser!).email;
    public bool isVerified => ((EmailStatusMessageEventParser)parser!).isVerified;
    public bool allowChange => ((EmailStatusMessageEventParser)parser!).allowChange;
}
