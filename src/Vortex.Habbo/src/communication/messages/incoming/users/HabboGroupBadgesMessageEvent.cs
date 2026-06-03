using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Users;

namespace Vortex.Habbo.Communication.Messages.Incoming.Users;

/// @see com.sulake.habbo.communication.messages.incoming.users.HabboGroupBadgesMessageEvent
public class HabboGroupBadgesMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(HabboGroupBadgesMessageEventParser))
{
    public Dictionary<int, string> badges => ((HabboGroupBadgesMessageEventParser)parser!).badges;
}
