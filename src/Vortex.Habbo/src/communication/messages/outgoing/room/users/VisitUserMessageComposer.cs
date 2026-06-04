using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Users;

/// @see com.sulake.habbo.communication.messages.outgoing.room.users.VisitUserMessageComposer
public class VisitUserMessageComposer(string userName) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [userName];
    }
}
