namespace Vortex.Room.Events;

/// <summary>
/// Generic typed event dispatched from room to objects.
/// </summary>
/// @see com.sulake.room.events.RoomToObjectEvent
public class RoomToObjectEvent(string type)
{
    public string Type { get; } = type;
}
