using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

/// @see com.sulake.habbo.communication.messages.incoming.handshake.UserObjectEvent
public class UserObjectEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(UserObjectMessageEventParser))
{
    private UserObjectMessageEventParser GetParser()
    {
        return (UserObjectMessageEventParser)parser!;
    }

    public int id => GetParser().id;
    public string name => GetParser().name;
    public string figure => GetParser().figure;
    public string sex => GetParser().sex;
    public string realName => GetParser().realName;
    public int respectLeft => GetParser().respectLeft;
    public int petRespectLeft => GetParser().petRespectLeft;
    public bool nameChangeAllowed => GetParser().nameChangeAllowed;
    public bool accountSafetyLocked => GetParser().accountSafetyLocked;
}
