using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Chat;

/// @see com.sulake.habbo.communication.messages.parser.room.chat.RemainingMutePeriodEventParser
public class RemainingMutePeriodEventParser : IMessageParser
{
    public int SecondsRemaining { get; private set; }

    public bool Flush()
    {
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        SecondsRemaining = param1.ReadInteger();
        return true;
    }
}
