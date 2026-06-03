using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Action;

/// @see com.sulake.habbo.communication.messages.parser.room.action.SleepMessageEventParser
public class SleepMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public bool Sleeping { get; private set; }

    public bool Flush() { UserId = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        UserId = param1.ReadInteger();
        Sleeping = param1.ReadBoolean();
        return true;
    }
}
