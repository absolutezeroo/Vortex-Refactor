using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Moderation;

/// @see com.sulake.habbo.communication.messages.outgoing.room.moderation.AmbassadorAlertMessageComposer
public class AmbassadorAlertMessageComposer(int userId) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [userId];
    }
}
