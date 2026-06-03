namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar typing indicator state change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarTypingUpdateMessage
public class RoomObjectAvatarTypingUpdateMessage(bool isTyping = false) : RoomObjectUpdateStateMessage
{
    public bool IsTyping => isTyping;
}
