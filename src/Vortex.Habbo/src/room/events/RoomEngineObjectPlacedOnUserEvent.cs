namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineObjectPlacedOnUserEvent
public class RoomEngineObjectPlacedOnUserEvent : RoomEngineObjectEvent
{
    public int DroppedObjectId { get; }

    public int DroppedObjectCategory { get; }

    /// AS3 note: Original has a bug where droppedObjectId uses its own getter instead of param5.
    /// We fix this by correctly assigning param5.
    public RoomEngineObjectPlacedOnUserEvent(
        string type,
        int roomId,
        int objectId,
        int category,
        int droppedObjectId,
        int droppedObjectCategory)
        : base(type, roomId, objectId, category)
    {
        DroppedObjectId = droppedObjectId;
        DroppedObjectCategory = droppedObjectCategory;
    }
}
