using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

using Array = Godot.Collections.Array;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class AuthenticationOkMessageEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(AuthenticationOkMessageEventParser))
{
    public Array suggestedLoginActions => ((AuthenticationOkMessageEventParser)parser!).suggestedLoginActions;
}
