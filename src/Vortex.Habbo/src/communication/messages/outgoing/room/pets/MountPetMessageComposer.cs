using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Pets;

/// @see com.sulake.habbo.communication.messages.outgoing.room.pets.MountPetMessageComposer
public class MountPetMessageComposer(int petId, bool mount) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [petId, mount];
    }
}
