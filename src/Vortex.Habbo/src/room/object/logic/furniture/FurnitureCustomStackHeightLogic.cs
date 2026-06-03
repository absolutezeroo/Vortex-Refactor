namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureCustomStackHeightLogic (class_3434)
public class FurnitureCustomStackHeightLogic : FurnitureMultiStateLogic
{
    public override string? Widget => "RWE_CUSTOM_STACK_HEIGHT";

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        Object?.ModelController.SetNumber("furniture_always_stackable", 1);
    }
}
