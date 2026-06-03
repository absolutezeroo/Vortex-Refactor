using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Session;

/// @see com.sulake.habbo.communication.messages.outgoing.room.session.QuitMessageComposer
public class QuitMessageComposer : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [];
    }
}
