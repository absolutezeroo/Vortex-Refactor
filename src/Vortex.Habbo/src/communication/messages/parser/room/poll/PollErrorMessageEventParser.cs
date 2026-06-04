// @see com.sulake.habbo.communication.messages.parser.room.poll._SafeStr_58

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Poll;

/// @see com.sulake.habbo.communication.messages.parser.room.poll._SafeStr_58
public class PollErrorMessageEventParser : IMessageParser
{
    public int ErrorCode { get; private set; }

    public bool Flush()
    {
        ErrorCode = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format from AS3 source (_SafeStr_58)
        ErrorCode = param1.ReadInteger();
        return true;
    }
}
