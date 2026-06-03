using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.RoomEntryInfoMessageEventParser
public class RoomEntryInfoMessageEventParser : IMessageParser
{
    public int GuestRoomId { get; private set; }
    public bool Owner { get; private set; }

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        GuestRoomId = param1.ReadInteger();
        Owner = param1.ReadBoolean();
        return true;
    }
}
