// @see com.sulake.habbo.communication.messages.parser.room.chat.HandItemReceivedMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.HandItemReceivedMessageParser
public class HandItemReceivedMessageEventParser : IMessageParser
{
    public int HandItemId { get; private set; }

    public bool Flush()
    {
        HandItemId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format from AS3 source
        HandItemId = param1.ReadInteger();
        return true;
    }
}
