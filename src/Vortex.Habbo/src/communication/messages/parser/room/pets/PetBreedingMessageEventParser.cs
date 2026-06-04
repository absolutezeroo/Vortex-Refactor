// @see com.sulake.habbo.communication.messages.parser.room.pets.PetBreedingMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetBreedingMessageParser
public class PetBreedingMessageEventParser : IMessageParser
{
    public int State { get; private set; }
    public int OwnPetId { get; private set; }
    public int OtherPetId { get; private set; }

    public bool Flush()
    {
        State = 0;
        OwnPetId = 0;
        OtherPetId = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        State = param1.ReadInteger();
        OwnPetId = param1.ReadInteger();
        OtherPetId = param1.ReadInteger();
        return true;
    }
}
