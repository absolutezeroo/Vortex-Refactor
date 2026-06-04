// @see com.sulake.habbo.communication.messages.parser.room.pets.PetInfoMessageParser

using Vortex.Core.Communication.Messages;
using Vortex.Habbo.Session;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetInfoMessageParser
public class PetInfoMessageEventParser : IMessageParser
{
    public PetInfo? PetInfo { get; private set; }

    public bool Flush()
    {
        PetInfo = null;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        var info = new PetInfo
        {
            petId              = param1.ReadInteger(),
            level              = param1.ReadInteger(),
            levelMax           = param1.ReadInteger(),
            experience         = param1.ReadInteger(),
            experienceMax      = param1.ReadInteger(),
            energy             = param1.ReadInteger(),
            energyMax          = param1.ReadInteger(),
            nutrition          = param1.ReadInteger(),
            nutritionMax       = param1.ReadInteger(),
            respect            = param1.ReadInteger(),
            ownerId            = param1.ReadInteger(),
            ownerName          = param1.ReadString(),
            age                = param1.ReadInteger(),
            breedId            = param1.ReadInteger(),
            rarityLevel        = param1.ReadInteger(),
            hasFreeSaddle      = param1.ReadBoolean(),
            isRiding           = param1.ReadBoolean(),
            canBreed           = param1.ReadBoolean(),
            canHarvest         = param1.ReadBoolean(),
            canRevive          = param1.ReadBoolean(),
            hasBreedingPermission = param1.ReadBoolean(),
            maxWellBeingSeconds         = param1.ReadInteger(),
            remainingWellBeingSeconds   = param1.ReadInteger(),
            remainingGrowingSeconds     = param1.ReadInteger(),
            accessRights       = param1.ReadInteger(),
        };
        int thresholdCount = param1.ReadInteger();
        var thresholds = new List<int>(thresholdCount);
        for (int i = 0; i < thresholdCount; i++)
        {
            thresholds.Add(param1.ReadInteger());
        }

        info.skillTresholds = thresholds;
        PetInfo = info;
        return true;
    }
}
