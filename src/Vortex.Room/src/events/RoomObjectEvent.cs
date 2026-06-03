using Vortex.Room.Object;

namespace Vortex.Room.Events;

/// <summary>
/// Base event for room object events. NOT a Flash Event subclass —
/// uses plain C# class with string type constants, matching Vortex.Core event dispatcher pattern.
/// </summary>
/// @see com.sulake.room.events.RoomObjectEvent
public class RoomObjectEvent(string type, IRoomObject? obj)
{
    public string Type { get; } = type;
    public IRoomObject? Object { get; } = obj;

    public int ObjectId => Object?.Id ?? -1;

    public string? ObjectType => Object?.Type;
}
