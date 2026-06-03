namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureRentableSpaceLogic (class_3504)
public class FurnitureRentableSpaceLogic : FurnitureLogic
{
    public override string? Widget => "RWE_RENTABLESPACE";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectDataRequestEvent.CURRENT_USER_ID];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is RoomObjectDataUpdateMessage && Object != null)
        {
            DispatchEvent(new RoomObjectDataRequestEvent(
                RoomObjectDataRequestEvent.CURRENT_USER_ID, Object));
        }
    }
}
