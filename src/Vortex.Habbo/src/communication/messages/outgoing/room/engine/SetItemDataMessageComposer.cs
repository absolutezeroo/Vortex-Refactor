using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;

/// @see com.sulake.habbo.communication.messages.outgoing.room.engine.SetItemDataMessageComposer
public class SetItemDataMessageComposer(int objectId, string colorHex = "", string data = "") : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [objectId, colorHex, data];
    }
}
