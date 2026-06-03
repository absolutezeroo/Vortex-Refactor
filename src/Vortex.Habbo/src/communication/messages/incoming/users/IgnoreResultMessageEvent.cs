using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Users;

namespace Vortex.Habbo.Communication.Messages.Incoming.Users;

/// @see com.sulake.habbo.communication.messages.incoming.users.IgnoreResultMessageEvent
public class IgnoreResultMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(IgnoreResultMessageEventParser))
{
    public int result => ((IgnoreResultMessageEventParser)parser!).result;
    public string name => ((IgnoreResultMessageEventParser)parser!).name;
}
