namespace Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.events.RoomEngineUseProductEvent
public class RoomEngineUseProductEvent(string type,
    int roomId,
    int objectId,
    int category,
    int inventoryStripId = -1,
    int furnitureTypeId = -1)
    : RoomEngineObjectEvent(type, roomId, objectId, category)
{
    public const string USE_PRODUCT_FROM_ROOM = "ROSM_USE_PRODUCT_FROM_ROOM";
    public const string USE_PRODUCT_FROM_INVENTORY = "ROSM_USE_PRODUCT_FROM_INVENTORY";

    public int InventoryStripId { get; } = inventoryStripId;

    public int FurnitureTypeId { get; } = furnitureTypeId;
}
