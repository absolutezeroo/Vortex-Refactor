using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Users;

/// @see com.sulake.habbo.communication.messages.outgoing.room.users.SetClothingChangeDataMessageComposer
public class SetClothingChangeDataMessageComposer(int stuffId, string figure, string figureType) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [stuffId, figure, figureType];
    }
}
