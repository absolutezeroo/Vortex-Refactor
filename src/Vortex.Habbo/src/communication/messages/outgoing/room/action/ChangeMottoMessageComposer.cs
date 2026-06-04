using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Action;

/// @see com.sulake.habbo.communication.messages.outgoing.room.action.ChangeMottoMessageComposer
public class ChangeMottoMessageComposer(string motto) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [motto];
    }
}
