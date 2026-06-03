using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Users;

namespace Vortex.Habbo.Communication.Messages.Incoming.Users;

/// @see com.sulake.habbo.communication.messages.incoming.users.GroupDetailsChangedMessageEvent
public class GroupDetailsChangedMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(GroupDetailsChangedMessageEventParser))
{
    public int groupId => ((GroupDetailsChangedMessageEventParser)parser!).groupId;
}
