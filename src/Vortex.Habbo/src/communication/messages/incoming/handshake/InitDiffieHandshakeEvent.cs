using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class InitDiffieHandshakeEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(InitDiffieHandshakeEventParser))
{
    public string encryptedPrime => ((InitDiffieHandshakeEventParser)parser!).encryptedPrime;

    public string encryptedGenerator => ((InitDiffieHandshakeEventParser)parser!).encryptedGenerator;
}
