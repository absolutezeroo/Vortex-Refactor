using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Users;

/// @see com.sulake.habbo.communication.messages.parser.users.AccountSafetyLockStatusChangeMessageParser
public class AccountSafetyLockMessageEventParser : IMessageParser
{
    public int status { get; private set; }

    public bool Flush()
    {
        status = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        status = param1.ReadInteger();
        return true;
    }
}
