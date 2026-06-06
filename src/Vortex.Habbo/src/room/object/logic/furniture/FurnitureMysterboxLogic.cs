namespace Vortex.Habbo.Room.Object.Logic;

using Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureMysterboxLogic (class_3392)
public class FurnitureMysterboxLogic : FurnitureMultiStateLogic
{
    public override string? ContextMenu => "MYSTERY_BOX";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.MYSTERYBOX_OPEN_DIALOG];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.MYSTERYBOX_OPEN_DIALOG, Object));
    }
}
