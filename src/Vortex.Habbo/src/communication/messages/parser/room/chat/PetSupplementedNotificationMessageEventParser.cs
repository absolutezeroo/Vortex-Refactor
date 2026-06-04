// @see com.sulake.habbo.communication.messages.parser.room.chat.PetSupplementedNotificationMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.PetSupplementedNotificationMessageParser
public class PetSupplementedNotificationMessageEventParser : IMessageParser
{
    public int PetId { get; private set; }
    public int SupplementId { get; private set; }

    public bool Flush()
    {
        PetId = 0;
        SupplementId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        PetId = param1.ReadInteger();
        SupplementId = param1.ReadInteger();
        return true;
    }
}
