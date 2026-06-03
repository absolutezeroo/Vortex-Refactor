using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Action;

/// @see com.sulake.habbo.communication.messages.parser.room.action.ExpressionMessageEventParser
public class ExpressionMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public int ExpressionType { get; private set; } = -1;

    public bool Flush() { UserId = 0; ExpressionType = -1; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        UserId = param1.ReadInteger();
        ExpressionType = param1.ReadInteger();
        return true;
    }
}
