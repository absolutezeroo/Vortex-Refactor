using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Session;

/// @see com.sulake.habbo.communication.messages.parser.room.session.HanditemConfigurationMessageEventParser
public class HanditemConfigurationMessageEventParser : IMessageParser
{
    public bool IsHanditemControlBlocked { get; private set; }

    public bool Flush() { IsHanditemControlBlocked = false; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        IsHanditemControlBlocked = param1.ReadBoolean();
        return true;
    }
}
