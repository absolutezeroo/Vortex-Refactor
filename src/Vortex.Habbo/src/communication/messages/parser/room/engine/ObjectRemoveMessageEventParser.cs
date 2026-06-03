using System.Globalization;

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.ObjectRemoveMessageEventParser
public class ObjectRemoveMessageEventParser : IMessageParser
{
    public int Id { get; private set; }
    public bool IsExpired { get; private set; }
    public int PickerId { get; private set; }
    public int Delay { get; private set; }

    public bool Flush() { Id = 0; Delay = 0; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }
        Id = int.Parse(param1.ReadString(), CultureInfo.InvariantCulture);
        IsExpired = param1.ReadBoolean();
        PickerId = param1.ReadInteger();
        Delay = param1.ReadInteger();
        return true;
    }
}
