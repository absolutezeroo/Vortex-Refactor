namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar gesture change (smile, sad, surprised, etc.).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarGestureUpdateMessage
public class RoomObjectAvatarGestureUpdateMessage(int gesture) : RoomObjectUpdateStateMessage
{
    public int Gesture => gesture;
}
