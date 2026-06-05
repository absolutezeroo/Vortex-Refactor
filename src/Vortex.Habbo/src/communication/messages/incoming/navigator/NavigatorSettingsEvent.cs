using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Navigator;

namespace Vortex.Habbo.Communication.Messages.Incoming.Navigator;

/// @see com.sulake.habbo.communication.messages.incoming.navigator.NavigatorSettingsEvent
public class NavigatorSettingsEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(NavigatorSettingsMessageParser))
{
    /// @see NavigatorSettingsMessageParser.as::homeRoomId
    public int HomeRoomId => ((NavigatorSettingsMessageParser)parser!).HomeRoomId;

    /// @see NavigatorSettingsMessageParser.as::roomIdToEnter
    public int RoomIdToEnter => ((NavigatorSettingsMessageParser)parser!).RoomIdToEnter;
}
