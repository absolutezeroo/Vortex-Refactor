using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Users;

/// @see com.sulake.habbo.communication.messages.parser.users.EmailStatusParser
public class EmailStatusMessageEventParser : IMessageParser
{
    public string email { get; private set; } = "";
    public bool isVerified { get; private set; }
    public bool allowChange { get; private set; }

    public bool Flush()
    {
        email = "";
        isVerified = false;
        allowChange = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        email = param1.ReadString();
        isVerified = param1.ReadBoolean();
        allowChange = param1.ReadBoolean();
        return true;
    }
}
