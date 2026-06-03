namespace Vortex.Habbo.Room.Messages;

/// <summary>
/// Avatar selection state change (clicked/deselected).
/// </summary>
/// @see com.sulake.habbo.room.messages.RoomObjectAvatarSelectedMessage
public class RoomObjectAvatarSelectedMessage(bool selected) : RoomObjectUpdateStateMessage
{
    public bool Selected => selected;
}
