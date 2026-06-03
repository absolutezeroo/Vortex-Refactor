using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.parser.room.furniture.OneWayDoorStatusMessageParser
public class OneWayDoorStatusMessageEventParser : IMessageParser
{
    public int Id { get; private set; } = -1;
    public int Status { get; private set; }

    public bool Flush() { Id = -1; Status = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = param1.ReadInteger();
        Status = param1.ReadInteger();
        return true;
    }
}
