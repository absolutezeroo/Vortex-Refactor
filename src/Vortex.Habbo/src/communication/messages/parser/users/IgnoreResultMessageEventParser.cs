using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Users;

/// @see com.sulake.habbo.communication.messages.parser.users.IgnoreResultMessageParser
public class IgnoreResultMessageEventParser : IMessageParser
{
    public int result { get; private set; } = -1;
    public string name { get; private set; } = "";

    public bool Flush()
    {
        result = -1;
        name = "";
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        result = param1.ReadInteger();
        name = param1.ReadString();
        return true;
    }
}
