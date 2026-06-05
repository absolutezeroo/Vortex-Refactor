using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Users;

namespace Vortex.Habbo.Communication.Messages.Incoming.Users;

/// @see com.sulake.habbo.communication.messages.incoming.users.class_217
public class AccountSafetyLockMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(AccountSafetyLockMessageEventParser))
{
    public int status => ((AccountSafetyLockMessageEventParser)parser!).status;
}
