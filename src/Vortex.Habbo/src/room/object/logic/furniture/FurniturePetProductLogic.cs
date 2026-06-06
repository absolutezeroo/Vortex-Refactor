namespace Vortex.Habbo.Room.Object.Logic;

using Events;
using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurniturePetProductLogic
public class FurniturePetProductLogic : FurnitureMultiStateLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.PET_PRODUCT_MENU];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        double realRoom = Object.Model.GetNumber("furniture_real_room_object");

        if (realRoom == 1)
        {
            Object.ModelController.SetString("RWEIEP_INFOSTAND_EXTRA_PARAM", "RWEIEP_USABLE_PRODUCT");
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.PET_PRODUCT_MENU, Object));
    }
}
