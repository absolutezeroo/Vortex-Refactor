// @see com.sulake.habbo.communication.messages.parser.room.engine.NewFriendRequestMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.NewFriendRequestMessageParser
public class NewFriendRequestMessageEventParser : IMessageParser
{
    public int RequestId { get; private set; }
    public int UserId { get; private set; }
    public string? UserName { get; private set; }

    public bool Flush()
    {
        RequestId = 0;
        UserId = 0;
        UserName = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        RequestId = param1.ReadInteger();
        UserId = param1.ReadInteger();
        UserName = param1.ReadString();
        return true;
    }
}
