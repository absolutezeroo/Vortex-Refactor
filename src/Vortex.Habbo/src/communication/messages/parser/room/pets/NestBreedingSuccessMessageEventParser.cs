// @see com.sulake.habbo.communication.messages.parser.room.pets.NestBreedingSuccessMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.NestBreedingSuccessMessageParser
public class NestBreedingSuccessMessageEventParser : IMessageParser
{
    public int PetId { get; private set; }
    public int RarityCategory { get; private set; }

    public bool Flush()
    {
        PetId = 0;
        RarityCategory = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        PetId = param1.ReadInteger();
        RarityCategory = param1.ReadInteger();
        return true;
    }
}
