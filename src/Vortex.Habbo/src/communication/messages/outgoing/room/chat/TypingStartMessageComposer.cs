using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Chat;

/// @see com.sulake.habbo.communication.messages.outgoing.room.chat._SafeStr_52
public class TypingStartMessageComposer : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload from AS3 source (_SafeStr_52)
        return [];
    }
}
