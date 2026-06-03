using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.MoveObjectMessageComposer
public class MoveObjectMessageComposer(int objectId, int x, int y, int direction) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [objectId, x, y, direction];
    }
}
