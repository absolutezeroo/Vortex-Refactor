using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Preferences;

namespace Vortex.Habbo.Communication.Messages.Incoming.Preferences;

/// @see com.sulake.habbo.communication.messages.incoming.preferences.class_219
public class AccountPreferencesMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(AccountPreferencesMessageEventParser))
{
    private AccountPreferencesMessageEventParser GetParser()
    {
        return (AccountPreferencesMessageEventParser)parser!;
    }

    public int traxVolume => GetParser().traxVolume;
    public int furniVolume => GetParser().furniVolume;
    public int uiVolume => GetParser().uiVolume;
    public bool freeFlowChatDisabled => GetParser().freeFlowChatDisabled;
    public bool roomInvitesIgnored => GetParser().roomInvitesIgnored;
    public bool roomCameraFollowDisabled => GetParser().roomCameraFollowDisabled;
    public int uiFlags => GetParser().uiFlags;
    public int preferredChatStyle => GetParser().preferredChatStyle;
}
