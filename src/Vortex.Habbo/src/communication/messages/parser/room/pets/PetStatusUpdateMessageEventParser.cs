// @see com.sulake.habbo.communication.messages.parser.room.pets.PetStatusUpdateMessageParser

using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Parser.Room.Pets;

/// @see com.sulake.habbo.communication.messages.parser.room.pets.PetStatusUpdateMessageParser
public class PetStatusUpdateMessageEventParser : IMessageParser
{
    public int RoomIndex { get; private set; }
    public int PetId { get; private set; }
    public bool CanBreed { get; private set; }
    public bool CanHarvest { get; private set; }
    public bool CanRevive { get; private set; }
    public bool HasBreedingPermission { get; private set; }

    public bool Flush()
    {
        RoomIndex = 0;
        PetId = 0;
        CanBreed = false;
        CanHarvest = false;
        CanRevive = false;
        HasBreedingPermission = false;
        return true;
    }

    public bool Parse(IMessageDataWrapper param1)
    {
        // TODO(as3-port): verify wire format order from AS3 source
        RoomIndex = param1.ReadInteger();
        PetId = param1.ReadInteger();
        CanBreed = param1.ReadBoolean();
        CanHarvest = param1.ReadBoolean();
        CanRevive = param1.ReadBoolean();
        HasBreedingPermission = param1.ReadBoolean();
        return true;
    }
}
