using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Room.Chat;

/// @see com.sulake.habbo.communication.messages.outgoing.room.chat.ChatMessageComposer
public class ChatMessageComposer(string text, int styleId = 0, int trackingId = -1) : IMessageComposer
{
    public void Dispose() { }
    public List<object> GetMessageArray()
    {
        // TODO(as3-port): verify payload order from AS3 source
        return [text, styleId, trackingId];
    }
}
