using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.ChatMessageEventParser
public class ChatMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public string Text { get; private set; } = "";
    public int Gesture { get; private set; }
    public int StyleId { get; private set; }
    public int TrackingId { get; private set; } = -1;
    public List<(string Url, string Text, bool TrustedUrl)>? Links { get; private set; }

    public bool Flush()
    {
        UserId = 0;
        Text = "";
        Gesture = 0;
        Links = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        UserId = param1.ReadInteger();
        Text = param1.ReadString();
        Gesture = param1.ReadInteger();
        StyleId = param1.ReadInteger();
        int linkCount = param1.ReadInteger();
        if (linkCount > 0)
        {
            Links = [];
            for (int i = 0; i < linkCount; i++)
            {
                Links.Add((param1.ReadString(), param1.ReadString(), param1.ReadBoolean()));
            }
        }
        TrackingId = param1.ReadInteger();
        return true;
    }
}
