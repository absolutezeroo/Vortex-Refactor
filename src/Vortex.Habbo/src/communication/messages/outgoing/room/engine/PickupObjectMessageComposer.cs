using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.PickupObjectMessageComposer
public class PickupObjectMessageComposer(int objectId, int category, bool isTrade = false) : IMessageComposer
{
    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        int pickupType = (category - 10) switch
        {
            0 => 2,
            10 => 1,
            _ => 0,
        };
        return pickupType == 0 ? [] : [pickupType, objectId, isTrade];
    }
}
