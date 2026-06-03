using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.UseWallItemMessageComposer
public class UseWallItemMessageComposer(int objectId, int param = 0) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [objectId, param];
    }
}
