namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineEvent
public class RoomEngineEvent(string type, int roomId)
{
    public const string ROOM_ENGINE_INITIALIZED = "REE_ENGINE_INITIALIZED";
    public const string ROOM_INITIALIZED = "REE_INITIALIZED";
    public const string ROOM_DISPOSED = "REE_DISPOSED";
    public const string ROOM_ENGINE_GAME_MODE = "REE_GAME_MODE";
    public const string ROOM_ENGINE_NORMAL_MODE = "REE_NORMAL_MODE";
    public const string ROOM_OBJECTS_INITIALIZED = "REE_OBJECTS_INITIALIZED";
    public const string ROOM_ZOOMED = "REE_ROOM_ZOOMED";
    public const string ROOM_ENTRANCE_AFTER_SPECTATE = "REE_ENTRANCE_AFTER_SPECTATE";

    public string Type { get; } = type;

    public int RoomId { get; } = roomId;

    /// @see RoomEngineEvent.as::type
    public string type => Type;

    /// @see RoomEngineEvent.as::roomId
    public int roomId => RoomId;
}
