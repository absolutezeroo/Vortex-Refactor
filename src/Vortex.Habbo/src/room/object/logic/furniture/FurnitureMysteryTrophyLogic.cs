namespace Vortex.Habbo.Room.Object.Logic;

using Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureMysteryTrophyLogic
public class FurnitureMysteryTrophyLogic : FurnitureMultiStateLogic
{
    public override string? ContextMenu => "MYSTERY_TROPHY";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.MYSTERYTROPHY_OPEN_DIALOG];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.MYSTERYTROPHY_OPEN_DIALOG, Object));
    }
}
