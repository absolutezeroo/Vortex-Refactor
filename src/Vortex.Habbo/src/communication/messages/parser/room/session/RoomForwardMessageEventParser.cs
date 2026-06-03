using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.RoomForwardMessageEventParser
public class RoomForwardMessageEventParser : IMessageParser
{
    public int RoomId { get; private set; }
    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        RoomId = param1.ReadInteger();
        return true;
    }
}
