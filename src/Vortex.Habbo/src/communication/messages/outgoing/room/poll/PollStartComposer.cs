using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Poll;

/// @see com.sulake.habbo.communication.messages.outgoing.room.poll.PollStartComposer
public class PollStartComposer(int pollId) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [pollId];
    }
}
