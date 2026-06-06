namespace Vortex.Habbo.Room.Object.Logic;

using Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureRandomStateLogic
public class FurnitureRandomStateLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_RANDOM];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectStateChangeEvent(
            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_RANDOM, Object));
    }
}
