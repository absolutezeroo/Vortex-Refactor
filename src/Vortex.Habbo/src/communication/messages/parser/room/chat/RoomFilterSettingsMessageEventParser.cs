using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.RoomFilterSettingsMessageEventParser
public class RoomFilterSettingsMessageEventParser : IMessageParser
{
    public List<string> BadWords { get; private set; } = [];

    public bool Flush()
    {
        return false;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        BadWords = [];
        int count = param1.ReadInteger();
        for (int i = 0; i < count; i++)
        {
            BadWords.Add(param1.ReadString());
        }
        return false;
    }
}
