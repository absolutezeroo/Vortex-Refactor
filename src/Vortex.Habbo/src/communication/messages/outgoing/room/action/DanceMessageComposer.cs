using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Action;

/// @see com.sulake.habbo.communication.messages.outgoing.room.action.DanceMessageComposer
public class DanceMessageComposer(int danceStyle) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [danceStyle];
    }
}
