namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar dance style change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarDanceUpdateMessage
public class RoomObjectAvatarDanceUpdateMessage(int danceStyle = 0) : RoomObjectUpdateStateMessage
{
    public int DanceStyle => danceStyle;
}
