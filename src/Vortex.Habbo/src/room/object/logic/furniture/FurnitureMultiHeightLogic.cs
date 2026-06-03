namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureMultiHeightLogic
public class FurnitureMultiHeightLogic : FurnitureMultiStateLogic
{
    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        Object?.ModelController.SetNumber("furniture_is_variable_height", 1);
    }
}
