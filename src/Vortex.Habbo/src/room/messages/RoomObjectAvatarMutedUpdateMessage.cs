namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar mute status change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarMutedUpdateMessage
public class RoomObjectAvatarMutedUpdateMessage(bool isMuted = false) : RoomObjectUpdateStateMessage
{
    public bool IsMuted => isMuted;
}
