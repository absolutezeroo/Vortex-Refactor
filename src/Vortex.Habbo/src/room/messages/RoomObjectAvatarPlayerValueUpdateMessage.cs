namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar player value update (game score, etc.).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarPlayerValueUpdateMessage
public class RoomObjectAvatarPlayerValueUpdateMessage(int value) : RoomObjectUpdateStateMessage
{
    public int Value => value;
}
