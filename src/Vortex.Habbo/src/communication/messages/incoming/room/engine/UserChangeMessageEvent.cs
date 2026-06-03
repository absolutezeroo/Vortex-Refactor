using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

namespace Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;

/// @see com.sulake.habbo.communication.messages.incoming.room.engine.UserChangeMessageEvent
public class UserChangeMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(UserChangeMessageEventParser))
{
    public int id => ((UserChangeMessageEventParser)parser!).Id;
    public string figure => ((UserChangeMessageEventParser)parser!).Figure;
    public string sex => ((UserChangeMessageEventParser)parser!).Sex;
}
