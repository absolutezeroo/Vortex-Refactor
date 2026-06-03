using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetExperienceParser
public class PetExperienceMessageEventParser : IMessageParser
{
    public int petId { get; private set; } = -1;
    public int petRoomIndex { get; private set; } = -1;
    public int gainedExperience { get; private set; }

    public bool Flush()
    {
        petId = -1;
        petRoomIndex = -1;
        gainedExperience = 0;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        if (param1 == null)
        {
            return false;
        }

        petId = param1.ReadInteger();
        petRoomIndex = param1.ReadInteger();
        gainedExperience = param1.ReadInteger();
        return true;
    }
}
