namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar chat bubble display with word count for animation timing.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarChatUpdateMessage
public class RoomObjectAvatarChatUpdateMessage(int numberOfWords) : RoomObjectUpdateStateMessage
{
    public int NumberOfWords => numberOfWords;
}
