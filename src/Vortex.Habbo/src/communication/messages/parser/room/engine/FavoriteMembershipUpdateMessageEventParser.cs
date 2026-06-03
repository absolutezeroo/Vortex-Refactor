using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.FavoriteMembershipUpdateMessageEventParser
public class FavoriteMembershipUpdateMessageEventParser : IMessageParser
{
    public int RoomIndex { get; private set; }
    public int HabboGroupId { get; private set; }
    public int Status { get; private set; }
    public string HabboGroupName { get; private set; } = "";

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        RoomIndex = param1.ReadInteger();
        HabboGroupId = param1.ReadInteger();
        Status = param1.ReadInteger();
        HabboGroupName = param1.ReadString();
        return true;
    }
}
