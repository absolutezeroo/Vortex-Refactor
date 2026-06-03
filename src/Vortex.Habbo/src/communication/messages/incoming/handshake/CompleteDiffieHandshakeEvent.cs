using System;

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;

namespace Vortex.Habbo.Communication.Messages.Incoming.Handshake;

public class CompleteDiffieHandshakeEvent(Action<IMessageEvent> param1) : MessageEvent(param1, typeof(CompleteDiffieHandshakeEventParser))
{
    public string encryptedPublicKey => ((CompleteDiffieHandshakeEventParser)parser!).encryptedPublicKey;
    public bool serverClientEncryption => ((CompleteDiffieHandshakeEventParser)parser!).serverClientEncryption;
}
