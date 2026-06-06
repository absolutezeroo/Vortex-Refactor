namespace Vortex.Habbo.Room.Object.Logic;

using Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureHabboWheelLogic
public class FurnitureHabboWheelLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectFurnitureActionEvent.USE_HABBOWHEEL];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.USE_HABBOWHEEL, Object));
    }
}
