using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Users;

/// @see com.sulake.habbo.communication.messages.parser.users._SafeStr_68
public class HabboGroupBadgesMessageEventParser : IMessageParser
{
    private readonly Dictionary<int, string> _badges = new();

    public Dictionary<int, string> badges => new(_badges);

    public bool Flush()
    {
        _badges.Clear();
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        _badges.Clear();
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            int groupId = param1.ReadInteger();
            string badge = param1.ReadString();
            _badges[groupId] = badge;
        }
        return true;
    }
}
