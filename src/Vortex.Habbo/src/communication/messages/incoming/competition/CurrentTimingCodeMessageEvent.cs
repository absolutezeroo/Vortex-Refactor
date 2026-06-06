using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Competition;

namespace Vortex.Habbo.Communication.Messages.Incoming.Competition;

/// @see com.sulake.habbo.communication.messages.incoming.competition.CurrentTimingCodeMessageEvent
public class CurrentTimingCodeMessageEvent(Action<IMessageEvent> param1)
    : MessageEvent(param1, typeof(CurrentTimingCodeMessageParser))
{
    /// @see CurrentTimingCodeMessageParser.as::schedulingStr
    public string SchedulingStr => ((CurrentTimingCodeMessageParser)parser!).SchedulingStr;

    /// @see CurrentTimingCodeMessageParser.as::code
    public string Code => ((CurrentTimingCodeMessageParser)parser!).Code;
}
