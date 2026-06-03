using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Help;

namespace Vortex.Habbo.Communication.Messages.Incoming.Help;

/// @see com.sulake.habbo.communication.messages.incoming.help.GuideSessionEndedMessageEvent
public class GuideSessionEndedMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(GuideSessionEndedMessageEventParser))
{
    public int endReason => ((GuideSessionEndedMessageEventParser)parser!).endReason;
}
