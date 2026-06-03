using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.RoomChatSettingsMessageEventParser
public class RoomChatSettingsMessageEventParser : IMessageParser
{
    public RoomChatSettings? ChatSettings { get; private set; }

    public bool Flush() { ChatSettings = null; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        ChatSettings = new RoomChatSettings(param1);
        return true;
    }
}

/// @see com.sulake.habbo.communication.messages.incoming.roomsettings.class_1732
public class RoomChatSettings(IMessageDataWrapper param1)
{
    public int Mode { get; } = param1.ReadInteger();
    public int BubbleWidth { get; } = param1.ReadInteger();
    public int ScrollSpeed { get; } = param1.ReadInteger();
    public int HearRange { get; } = param1.ReadInteger();
    public int FloodSensitivity { get; } = param1.ReadInteger();
}
