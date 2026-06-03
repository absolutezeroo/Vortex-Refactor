namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureTrophyLogic
public class FurnitureTrophyLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.TROPHY];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.TROPHY, Object));
    }
}
