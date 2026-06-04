// @see com.sulake.habbo.communication.messages.parser.room.engine.UserNameChangedMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.UserNameChangedMessageParser
public class UserNameChangedMessageEventParser : IMessageParser
{
    public int RoomIndex { get; private set; }
    public int UserId { get; private set; }
    public string? NewName { get; private set; }

    public bool Flush()
    {
        RoomIndex = 0;
        UserId = 0;
        NewName = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        RoomIndex = param1.ReadInteger();
        UserId = param1.ReadInteger();
        NewName = param1.ReadString();
        return true;
    }
}
