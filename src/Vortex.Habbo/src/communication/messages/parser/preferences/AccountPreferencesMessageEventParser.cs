using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Preferences;

/// @see com.sulake.habbo.communication.messages.parser.preferences.AccountPreferencesParser
public class AccountPreferencesMessageEventParser : IMessageParser
{
    public int traxVolume { get; private set; }
    public int furniVolume { get; private set; }
    public int uiVolume { get; private set; }
    public bool freeFlowChatDisabled { get; private set; }
    public bool roomInvitesIgnored { get; private set; }
    public bool roomCameraFollowDisabled { get; private set; }
    public int uiFlags { get; private set; }
    public int preferredChatStyle { get; private set; }

    public bool Flush()
    {
        traxVolume = 0;
        furniVolume = 0;
        uiVolume = 0;
        freeFlowChatDisabled = false;
        roomInvitesIgnored = false;
        roomCameraFollowDisabled = false;
        uiFlags = 0;
        preferredChatStyle = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        traxVolume = param1.ReadInteger();
        furniVolume = param1.ReadInteger();
        uiVolume = param1.ReadInteger();
        freeFlowChatDisabled = param1.ReadBoolean();
        roomInvitesIgnored = param1.ReadBoolean();
        roomCameraFollowDisabled = param1.ReadBoolean();
        uiFlags = param1.ReadInteger();
        preferredChatStyle = param1.ReadInteger();
        return true;
    }
}
