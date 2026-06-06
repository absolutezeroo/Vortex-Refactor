namespace Vortex.Habbo.Room.Object.Logic;

using Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureOneWayDoorLogic
public class FurnitureOneWayDoorLogic : FurnitureMultiStateLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectFurnitureActionEvent.ENTER_ONEWAYDOOR];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.ENTER_ONEWAYDOOR, Object));
    }
}
