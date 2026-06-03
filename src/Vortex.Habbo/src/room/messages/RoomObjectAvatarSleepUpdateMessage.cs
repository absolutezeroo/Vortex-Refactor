namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar idle/sleep state change (Zzz indicator).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarSleepUpdateMessage
public class RoomObjectAvatarSleepUpdateMessage(bool isSleeping = false) : RoomObjectUpdateStateMessage
{
    public bool IsSleeping => isSleeping;
}
