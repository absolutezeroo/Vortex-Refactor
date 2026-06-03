namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureEffectboxLogic (class_3391)
public class FurnitureEffectboxLogic : FurnitureMultiStateLogic
{
    public override string? ContextMenu => "EFFECT_BOX";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.EFFECTBOX_OPEN_DIALOG];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.EFFECTBOX_OPEN_DIALOG, Object));
    }
}
