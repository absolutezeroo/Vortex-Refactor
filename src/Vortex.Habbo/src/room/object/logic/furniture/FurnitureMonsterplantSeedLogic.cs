namespace Vortex.Habbo.Room.Object.Logic;

using Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureMonsterplantSeedLogic (class_3436)
public class FurnitureMonsterplantSeedLogic : FurnitureMultiStateLogic
{
    public override string? ContextMenu => "MONSTERPLANT_SEED";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG, Object));
    }
}
