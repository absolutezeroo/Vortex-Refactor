using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Permissions;

/// @see com.sulake.habbo.communication.messages.parser.room.permissions.YouAreControllerMessageParser
public class YouAreControllerMessageEventParser : IMessageParser
{
    public int FlatId { get; private set; }
    public int RoomControllerLevel { get; private set; }

    public bool Flush()
    {
        FlatId = 0;
        RoomControllerLevel = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        FlatId = param1.ReadInteger();
        RoomControllerLevel = param1.ReadInteger();
        return true;
    }
}
