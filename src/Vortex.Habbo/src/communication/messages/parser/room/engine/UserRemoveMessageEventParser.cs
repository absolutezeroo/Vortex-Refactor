using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.UserRemoveMessageEventParser
public class UserRemoveMessageEventParser : IMessageParser
{
    public int Id { get; private set; }

    public bool Flush() { Id = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = int.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        return true;
    }
}
