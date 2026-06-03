using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.GetItemDataMessageComposer
public class GetItemDataMessageComposer(int objectId) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [objectId];
    }
}
