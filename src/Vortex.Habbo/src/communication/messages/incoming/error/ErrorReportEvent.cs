using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Error;

namespace Vortex.Habbo.Communication.Messages.Incoming.Error;

public class ErrorReportEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(ErrorReportEventParser))
{
    public ErrorReportEventParser GetParser()
    {
        return (ErrorReportEventParser)parser!;
    }
}
