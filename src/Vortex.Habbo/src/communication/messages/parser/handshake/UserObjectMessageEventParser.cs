using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Handshake;

/// @see com.sulake.habbo.communication.messages.parser.handshake.UserObjectMessageParser
public class UserObjectMessageEventParser : IMessageParser
{
    public int id { get; private set; } = -1;
    public string name { get; private set; } = "";
    public string figure { get; private set; } = "";
    public string sex { get; private set; } = "";
    public string customData { get; private set; } = "";
    public string realName { get; private set; } = "";
    public bool directMail { get; private set; }
    public int respectTotal { get; private set; }
    public int respectLeft { get; private set; }
    public int petRespectLeft { get; private set; }
    public bool streamPublishingAllowed { get; private set; }
    public string lastAccessDate { get; private set; } = "";
    public bool nameChangeAllowed { get; private set; }
    public bool accountSafetyLocked { get; private set; }

    public bool Flush()
    {
        id = -1;
        name = "";
        figure = "";
        sex = "";
        customData = "";
        realName = "";
        directMail = false;
        respectTotal = 0;
        respectLeft = 0;
        petRespectLeft = 0;
        streamPublishingAllowed = false;
        lastAccessDate = "";
        nameChangeAllowed = false;
        accountSafetyLocked = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        id = param1.ReadInteger();
        name = param1.ReadString();
        figure = param1.ReadString();
        sex = param1.ReadString();
        customData = param1.ReadString();
        realName = param1.ReadString();
        directMail = param1.ReadBoolean();
        respectTotal = param1.ReadInteger();
        respectLeft = param1.ReadInteger();
        petRespectLeft = param1.ReadInteger();
        streamPublishingAllowed = param1.ReadBoolean();
        lastAccessDate = param1.ReadString();
        nameChangeAllowed = param1.ReadBoolean();
        accountSafetyLocked = param1.ReadBoolean();
        return true;
    }
}
