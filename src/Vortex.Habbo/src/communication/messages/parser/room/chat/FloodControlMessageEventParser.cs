using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.FloodControlMessageEventParser
public class FloodControlMessageEventParser : IMessageParser
{
    public int Seconds { get; private set; }

    public bool Flush() { Seconds = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Seconds = param1.ReadInteger();
        return true;
    }
}
