using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Pets;

/// @see com.sulake.habbo.communication.messages.outgoing.room.pets.HarvestPetMessageComposer
public class HarvestPetMessageComposer(int petId) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [petId];
    }
}
