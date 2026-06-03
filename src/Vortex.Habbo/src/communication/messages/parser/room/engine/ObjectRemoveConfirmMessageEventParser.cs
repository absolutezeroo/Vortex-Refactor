using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectRemoveConfirmMessageEventParser
public class ObjectRemoveConfirmMessageEventParser : IMessageParser
{
    public int Category { get; private set; }
    public int Id { get; private set; }
    public string? ConfirmTitle { get; private set; }
    public string? ConfirmBody { get; private set; }

    public bool Flush()
    {
        Category = 0;
        Id = 0;
        ConfirmTitle = null;
        ConfirmBody = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Category = param1.ReadInteger() == 1 ? 20 : 10;
        Id = param1.ReadInteger();
        ConfirmTitle = param1.ReadString();
        ConfirmBody = param1.ReadString();
        return true;
    }
}
