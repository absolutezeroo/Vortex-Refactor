// @see com.sulake.habbo.communication.messages.parser.room.pets.PetBreedingResultMessageParser

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetBreedingResultMessageParser
public class PetBreedingResultMessageEventParser : IMessageParser
{
    public PetBreedingResultData? OwnResult { get; private set; }
    public PetBreedingResultData? OtherResult { get; private set; }

    public bool Flush()
    {
        OwnResult = null;
        OtherResult = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        OwnResult = ParseResultData(param1);
        OtherResult = ParseResultData(param1);
        return true;
    }

    private static PetBreedingResultData ParseResultData(IMessageDataWrapper param1)
    {
        int stuffId      = param1.ReadInteger();
        int classId      = param1.ReadInteger();
        string productCode = param1.ReadString();
        int userId       = param1.ReadInteger();
        string userName  = param1.ReadString();
        int rarityLevel  = param1.ReadInteger();
        bool hasMutation = param1.ReadBoolean();
        return new PetBreedingResultData(stuffId, classId, productCode, userId, userName, rarityLevel, hasMutation);
    }
}
