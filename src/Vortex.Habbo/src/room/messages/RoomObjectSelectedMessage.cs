namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Generic room object selection state change.
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectSelectedMessage
public class RoomObjectSelectedMessage(bool selected) : RoomObjectUpdateStateMessage
{
    public bool Selected => selected;
}
