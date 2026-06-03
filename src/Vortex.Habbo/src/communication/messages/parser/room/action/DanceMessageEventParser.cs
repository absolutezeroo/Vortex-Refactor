using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Action;

/// @see com.sulake.habbo.communication.messages.parser.room.action.DanceMessageEventParser
public class DanceMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public int DanceStyle { get; private set; }

    public bool Flush() { UserId = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        UserId = param1.ReadInteger();
        DanceStyle = param1.ReadInteger();
        return true;
    }
}
