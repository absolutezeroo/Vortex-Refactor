using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Help;

namespace Vortex.Habbo.Communication.Messages.Incoming.Help;

/// @see com.sulake.habbo.communication.messages.incoming.help.GuideSessionStartedMessageEvent
public class GuideSessionStartedMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(GuideSessionStartedMessageEventParser))
{
    public int requesterUserId => ((GuideSessionStartedMessageEventParser)parser!).requesterUserId;
    public string requesterName => ((GuideSessionStartedMessageEventParser)parser!).requesterName;
    public string requesterFigure => ((GuideSessionStartedMessageEventParser)parser!).requesterFigure;
    public int guideUserId => ((GuideSessionStartedMessageEventParser)parser!).guideUserId;
    public string guideName => ((GuideSessionStartedMessageEventParser)parser!).guideName;
    public string guideFigure => ((GuideSessionStartedMessageEventParser)parser!).guideFigure;
}
