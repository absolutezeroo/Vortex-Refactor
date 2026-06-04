// @see com.sulake.habbo.communication.messages.parser.room.pets.ConfirmBreedingResultMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.ConfirmBreedingResultMessageParser
public class ConfirmBreedingResultMessageEventParser : IMessageParser
{
    public int BreedingNestStuffId { get; private set; }
    public int Result { get; private set; }

    public bool Flush()
    {
        BreedingNestStuffId = 0;
        Result = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        BreedingNestStuffId = param1.ReadInteger();
        Result = param1.ReadInteger();
        return true;
    }
}
