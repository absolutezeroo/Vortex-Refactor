using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Permissions;

/// @see com.sulake.habbo.communication.messages.parser.room.permissions.YouAreOwnerMessageParser
public class YouAreOwnerMessageEventParser : IMessageParser
{
    public int FlatId { get; private set; }

    public bool Flush() { FlatId = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        FlatId = param1.ReadInteger();
        return true;
    }
}
