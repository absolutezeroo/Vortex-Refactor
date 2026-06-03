using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Layout;

/// @see com.sulake.habbo.communication.messages.parser.room.layout.RoomEntryTileMessageParser
public class RoomEntryTileMessageEventParser : IMessageParser
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Dir { get; private set; }

    public bool Flush()
    {
        X = 0;
        Y = 0;
        Dir = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        X = param1.ReadInteger();
        Y = param1.ReadInteger();
        Dir = param1.ReadInteger();
        return true;
    }
}
