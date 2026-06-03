namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar sign display (numbered signs 0-17).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarSignUpdateMessage
public class RoomObjectAvatarSignUpdateMessage(int signType) : RoomObjectUpdateStateMessage
{
    public int SignType => signType;
}
