using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.YouArePlayingGameMessageEventParser
public class YouArePlayingGameMessageEventParser : IMessageParser
{
    public bool IsPlaying { get; private set; }

    public bool Flush() { IsPlaying = false; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        IsPlaying = param1.ReadBoolean();
        return true;
    }
}
