using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.UserTypingMessageEventParser
public class UserTypingMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public bool IsTyping { get; private set; }

    public bool Flush() { UserId = 0; IsTyping = false; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        UserId = param1.ReadInteger();
        IsTyping = param1.ReadInteger() == 1;
        return true;
    }
}
