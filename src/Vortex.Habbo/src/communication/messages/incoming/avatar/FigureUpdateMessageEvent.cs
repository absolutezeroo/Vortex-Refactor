using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Avatar;

namespace Vortex.Habbo.Communication.Messages.Incoming.Avatar;

/// @see com.sulake.habbo.communication.messages.incoming.avatar.class_199
public class FigureUpdateMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(FigureUpdateMessageEventParser))
{
    public string figure => ((FigureUpdateMessageEventParser)parser!).figure;
    public string gender => ((FigureUpdateMessageEventParser)parser!).gender;
}
