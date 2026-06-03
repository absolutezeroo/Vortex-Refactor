using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.parser.room.furniture.CustomStackingHeightUpdateMessageParser
public class CustomStackingHeightUpdateMessageEventParser : IMessageParser
{
    public int FurniId { get; private set; } = -1;
    public double Height { get; private set; }

    public bool Flush() { FurniId = -1; Height = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        FurniId = param1.ReadInteger();
        int rawHeight = param1.ReadInteger();
        Height = rawHeight / 100.0;
        return true;
    }
}
