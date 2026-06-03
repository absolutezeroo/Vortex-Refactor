using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Help;

namespace Vortex.Habbo.Communication.Messages.Incoming.Help;

/// @see com.sulake.habbo.communication.messages.incoming.help.GuideSessionErrorMessageEvent
public class GuideSessionErrorMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(GuideSessionErrorMessageEventParser))
{
    public int errorCode => ((GuideSessionErrorMessageEventParser)parser!).errorCode;
}
