namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Pet-specific gesture update (string-based unlike human gesture which is int).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarPetGestureUpdateMessage
public class RoomObjectAvatarPetGestureUpdateMessage(string gesture) : RoomObjectUpdateStateMessage
{
    public string Gesture => gesture;
}
