using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Engine;

/// @see com.sulake.habbo.communication.messages.parser.room.engine.SpecialRoomEffectMessageEventParser
public class SpecialRoomEffectMessageEventParser : IMessageParser
{
    public int EffectId { get; private set; } = -1;

    public bool Flush() { EffectId = -1; return true; }

    public bool Parse(IMessageDataWrapper param1)
    {
        EffectId = param1.ReadInteger();
        return true;
    }
}
