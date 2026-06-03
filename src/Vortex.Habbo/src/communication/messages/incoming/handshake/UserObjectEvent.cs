using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

/// @see com.sulake.habbo.communication.messages.incoming.handshake.UserObjectEvent
public class UserObjectEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(UserObjectMessageEventParser))
{
    public int id => ((UserObjectMessageEventParser)parser!).id;
    public string name => ((UserObjectMessageEventParser)parser!).name;
    public string figure => ((UserObjectMessageEventParser)parser!).figure;
    public string sex => ((UserObjectMessageEventParser)parser!).sex;
}
