using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

/// @see com.sulake.habbo.communication.messages.incoming.handshake.NoobnessLevelMessageEvent
public class NoobnessLevelMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(NoobnessLevelMessageEventParser))
{
    public int noobnessLevel => ((NoobnessLevelMessageEventParser)parser!).noobnessLevel;
}
