using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

/// @see com.sulake.habbo.communication.messages.parser.handshake.UserRightsMessageParser
public class UserRightsMessageEventParser : IMessageParser
{
    public int clubLevel { get; private set; }
    public int securityLevel { get; private set; }
    public bool isAmbassador { get; private set; }

    public bool Flush()
    {
        clubLevel = 0;
        securityLevel = 0;
        isAmbassador = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        clubLevel = param1.ReadInteger();
        securityLevel = param1.ReadInteger();
        isAmbassador = param1.ReadBoolean();
        return true;
    }
}
