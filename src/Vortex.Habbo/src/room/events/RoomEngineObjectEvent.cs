namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineObjectEvent
public class RoomEngineObjectEvent(string type, int roomId, int objectId, int category)
    : RoomEngineEvent(type, roomId)
{
    public const string SELECTED = "REOE_SELECTED";
    public const string DESELECTED = "REOE_DESELECTED";
    public const string ADDED = "REOE_ADDED";
    public const string REMOVED = "REOE_REMOVED";
    public const string PLACED = "REOE_PLACED";
    public const string PLACED_ON_USER = "REOE_PLACED_ON_USER";
    public const string CONTENT_UPDATED = "REOE_CONTENT_UPDATED";
    public const string REQUEST_MOVE = "REOE_REQUEST_MOVE";
    public const string REQUEST_ROTATE = "REOE_REQUEST_ROTATE";
    public const string REQUEST_PICKUP = "REOE_REQUEST_PICKUP";
    public const string MOUSE_ENTER = "REOE_MOUSE_ENTER";
    public const string MOUSE_LEAVE = "REOE_MOUSE_LEAVE";

    public int ObjectId { get; } = objectId;

    public int Category { get; } = category;
}
