// @see com.sulake.habbo.communication.messages.parser.room.engine.HabboUserBadgesMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.HabboUserBadgesMessageParser
public class HabboUserBadgesMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public IReadOnlyList<string> Badges { get; private set; } = [];

    public bool Flush()
    {
        UserId = 0;
        Badges = [];
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        UserId = param1.ReadInteger();
        int count = param1.ReadInteger();
        List<string> badges = new List<string>(count);
        for (int i = 0; i < count; i++)
        {
            param1.ReadInteger(); // slot index
            badges.Add(param1.ReadString());
        }
        Badges = badges;
        return true;
    }
}
