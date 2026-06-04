// @see com.sulake.habbo.communication.messages.parser.room.engine.DoorbellMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.DoorbellMessageParser
public class DoorbellMessageEventParser : IMessageParser
{
    public string? UserName { get; private set; }

    public bool Flush()
    {
        UserName = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format from AS3 source
        UserName = param1.ReadString();
        return true;
    }
}
