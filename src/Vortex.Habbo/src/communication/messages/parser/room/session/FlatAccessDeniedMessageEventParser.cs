// @see com.sulake.habbo.communication.messages.parser.room.session.FlatAccessDeniedMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.FlatAccessDeniedMessageParser
public class FlatAccessDeniedMessageEventParser : IMessageParser
{
    public int FlatId { get; private set; }
    public string? Info { get; private set; }

    public bool Flush()
    {
        FlatId = 0;
        Info = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        FlatId = param1.ReadInteger();
        Info = param1.ReadString();
        return true;
    }
}
