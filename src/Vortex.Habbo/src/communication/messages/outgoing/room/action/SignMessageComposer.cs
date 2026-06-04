using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Action;

/// @see com.sulake.habbo.communication.messages.outgoing.room.action.SignMessageComposer
public class SignMessageComposer(int sign) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [sign];
    }
}
