using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Chat;

/// @see com.sulake.habbo.communication.messages.outgoing.room.chat.WhisperMessageComposer
public class WhisperMessageComposer(string recipientName, string text, int styleId = 0) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [recipientName, text, styleId];
    }
}
