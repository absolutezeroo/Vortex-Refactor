// @see com.sulake.habbo.communication.messages.incoming.roomsettings.RoomModerationSettings

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Incoming.RoomSettings;

/// @see com.sulake.habbo.communication.messages.incoming.roomsettings.RoomModerationSettings
public class RoomModerationSettings
{
    public const int _SafeStr_1834 = 0;
    public const int _SafeStr_1835 = 1;
    public const int _SafeStr_1836 = 2;
    public const int _SafeStr_1837 = 4;
    public const int _SafeStr_1838 = 5;

    /// @see RoomModerationSettings.as::RoomModerationSettings
    public RoomModerationSettings(IMessageDataWrapper wrapper)
    {
        whoCanMute = wrapper.ReadInteger();
        whoCanKick = wrapper.ReadInteger();
        whoCanBan = wrapper.ReadInteger();
    }

    /// @see RoomModerationSettings.as::get whoCanMute
    public int whoCanMute { get; }

    /// @see RoomModerationSettings.as::get whoCanKick
    public int whoCanKick { get; }

    /// @see RoomModerationSettings.as::get whoCanBan
    public int whoCanBan { get; }
}
