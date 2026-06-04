// @see com.sulake.habbo.communication.messages.parser.room.chat.RespectNotificationMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.RespectNotificationMessageParser
public class RespectNotificationMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public int TotalRespects { get; private set; }

    public bool Flush()
    {
        UserId = 0;
        TotalRespects = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        UserId = param1.ReadInteger();
        TotalRespects = param1.ReadInteger();
        return true;
    }
}
