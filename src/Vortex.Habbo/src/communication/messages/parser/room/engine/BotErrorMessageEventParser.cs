// @see com.sulake.habbo.communication.messages.parser.room.engine.BotErrorMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.BotErrorMessageParser
public class BotErrorMessageEventParser : IMessageParser
{
    public int ErrorCode { get; private set; }

    public bool Flush()
    {
        ErrorCode = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format from AS3 source
        ErrorCode = param1.ReadInteger();
        return true;
    }
}
