using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.CantConnectMessageEventParser
public class CantConnectMessageEventParser : IMessageParser
{
    public const int REASON_FULL = 1;
    public const int REASON_CLOSED = 2;
    public const int REASON_BANNED = 3;
    public const int REASON_QUEUE = 4;

    public int Reason { get; private set; }
    public string Parameter { get; private set; } = "";

    public bool Flush()
    {
        Reason = 0;
        Parameter = "";
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        Reason = param1.ReadInteger();
        Parameter = Reason == 3 ? param1.ReadString() : "";
        return true;
    }
}
