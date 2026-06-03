using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.RoomReadyMessageEventParser
public class RoomReadyMessageEventParser : IMessageParser
{
    public string RoomType { get; private set; } = "";
    public int RoomId { get; private set; }

    public bool Flush()
    {
        RoomType = "";
        RoomId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        RoomType = param1.ReadString();
        RoomId = param1.ReadInteger();
        return true;
    }
}
