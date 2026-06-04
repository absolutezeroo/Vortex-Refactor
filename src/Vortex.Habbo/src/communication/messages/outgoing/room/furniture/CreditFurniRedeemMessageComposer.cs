using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Furniture;

/// @see com.sulake.habbo.communication.messages.outgoing.room.furniture.CreditFurniRedeemMessageComposer
public class CreditFurniRedeemMessageComposer(int stuffId) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [stuffId];
    }
}
