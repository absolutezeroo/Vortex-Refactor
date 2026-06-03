namespace Vortex.Room.Events;

/// @see com.sulake.room.events.RoomContentLoadedEvent
public class RoomContentLoadedEvent(string type, string contentType)
{
    public const string CONTENT_LOAD_SUCCESS = "RCLE_SUCCESS";
    public const string CONTENT_LOAD_FAILURE = "RCLE_FAILURE";
    public const string CONTENT_LOAD_CANCEL = "RCLE_CANCEL";

    public string Type { get; } = type;
    public string ContentType { get; } = contentType;
}
