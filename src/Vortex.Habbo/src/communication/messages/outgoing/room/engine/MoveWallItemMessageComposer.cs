using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.MoveWallItemMessageComposer
public class MoveWallItemMessageComposer(int itemId, string location) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [itemId, location];
    }
}
