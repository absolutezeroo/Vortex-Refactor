using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.FlatAccessibleMessageEventParser
public class FlatAccessibleMessageEventParser : IMessageParser
{
    public int FlatId { get; private set; }
    public string? UserName { get; private set; }

    public bool Flush()
    {
        UserName = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        FlatId = param1.ReadInteger();
        UserName = param1.ReadString();
        return true;
    }
}
