using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Competition;

/// @see com.sulake.habbo.communication.messages.parser.competition.CurrentTimingCodeMessageParser
public class CurrentTimingCodeMessageParser : IMessageParser
{
    /// @see CurrentTimingCodeMessageParser.as::schedulingStr
    public string SchedulingStr { get; private set; } = string.Empty;

    /// @see CurrentTimingCodeMessageParser.as::code
    public string Code { get; private set; } = string.Empty;

    /// @see CurrentTimingCodeMessageParser.as::flush
    public bool Flush()
    {
        SchedulingStr = string.Empty;
        Code = string.Empty;

        return true;
    }

    /// @see CurrentTimingCodeMessageParser.as::parse
    public bool Parse(IMessageDataWrapper param1)
    {
        SchedulingStr = param1.ReadString();
        Code = param1.ReadString();

        return true;
    }
}
