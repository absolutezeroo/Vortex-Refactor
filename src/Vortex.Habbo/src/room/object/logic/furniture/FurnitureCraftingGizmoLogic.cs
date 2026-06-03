namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureCraftingGizmoLogic (class_3469)
public class FurnitureCraftingGizmoLogic : FurnitureLogic
{
    public override string? Widget => "RWE_CRAFTING";

    public void SetAnimationState(int state)
    {
        Object?.ModelController.SetNumber("furniture_automatic_state_index", state);
    }
}
