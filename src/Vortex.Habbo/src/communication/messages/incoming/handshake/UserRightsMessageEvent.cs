using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

/// @see com.sulake.habbo.communication.messages.incoming.handshake.class_143
public class UserRightsMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(UserRightsMessageEventParser))
{
    public int clubLevel => ((UserRightsMessageEventParser)parser!).clubLevel;
    public int securityLevel => ((UserRightsMessageEventParser)parser!).securityLevel;
    public bool isAmbassador => ((UserRightsMessageEventParser)parser!).isAmbassador;
}
