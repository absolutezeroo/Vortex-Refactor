namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureCrackableLogic (class_3399)
public class FurnitureCrackableLogic : FurnitureLogic
{
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
            Object.ModelController.SetString("RWEIEP_INFOSTAND_EXTRA_PARAM", "RWEIEP_CRACKABLE_FURNI");
        }
    }
}
