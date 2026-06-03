using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.ClickFurniMessageComposer
public class ClickFurniMessageComposer(int objectId, int type = 0) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [objectId, type];
    }
}
