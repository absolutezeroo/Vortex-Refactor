using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Users;

/// @see com.sulake.habbo.communication.messages.parser.users.GroupDetailsChangedMessageParser
public class GroupDetailsChangedMessageEventParser : IMessageParser
{
    public int groupId { get; private set; }

    public bool Flush()
    {
        groupId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        groupId = param1.ReadInteger();
        return true;
    }
}
