// @see com.sulake.habbo.communication.messages.parser.room.chat.PetRespectNotificationMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.PetRespectNotificationMessageParser
public class PetRespectNotificationMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public int TotalRespects { get; private set; }
    public int PetId { get; private set; }
    public int PetRespects { get; private set; }

    public bool Flush()
    {
        UserId = 0;
        TotalRespects = 0;
        PetId = 0;
        PetRespects = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        UserId = param1.ReadInteger();
        TotalRespects = param1.ReadInteger();
        PetId = param1.ReadInteger();
        PetRespects = param1.ReadInteger();
        return true;
    }
}
