using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Action;

/// @see com.sulake.habbo.communication.messages.parser.room.action.UseObjectMessageEventParser
public class UseObjectMessageEventParser : IMessageParser
{
    public int UserId { get; private set; }
    public int ItemType { get; private set; }

    public bool Flush() { UserId = 0; ItemType = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        UserId = param1.ReadInteger();
        ItemType = param1.ReadInteger();
        return true;
    }
}
