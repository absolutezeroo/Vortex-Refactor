using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Users;

/// @see com.sulake.habbo.communication.messages.outgoing.room.users.NewUserExperienceScriptProceedComposer
public class NewUserExperienceScriptProceedComposer : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload from AS3 source
        return [];
    }
}
