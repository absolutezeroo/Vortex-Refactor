using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Navigator;

/// @see com.sulake.habbo.communication.messages.parser.navigator.NavigatorSettingsMessageParser
public class NavigatorSettingsMessageParser : IMessageParser
{
    /// @see NavigatorSettingsMessageParser.as::homeRoomId
    public int HomeRoomId { get; private set; }

    /// @see NavigatorSettingsMessageParser.as::roomIdToEnter
    public int RoomIdToEnter { get; private set; }

    /// @see NavigatorSettingsMessageParser.as::flush
    public bool Flush()
    {
        HomeRoomId = 0;
        RoomIdToEnter = 0;

        return true;
    }

    /// @see NavigatorSettingsMessageParser.as::parse
    public bool Parse(IMessageDataWrapper param1)
    {
        HomeRoomId = param1.ReadInteger();
        RoomIdToEnter = param1.ReadInteger();

        return true;
    }
}
