using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Session;

/// @see com.sulake.habbo.communication.messages.outgoing.room.session.OpenFlatConnectionMessageComposer
public class OpenFlatConnectionMessageComposer(int roomId, string password = "", int checksum = -1) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        return [roomId, password, checksum];
    }
}
