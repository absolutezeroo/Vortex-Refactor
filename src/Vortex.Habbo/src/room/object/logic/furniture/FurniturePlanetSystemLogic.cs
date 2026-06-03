namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

/// @see com.sulake.habbo.room.object.logic.furniture.FurniturePlanetSystemLogic
public class FurniturePlanetSystemLogic : FurnitureLogic
{
    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        XElement? planetSystem = xml.Element("planetsystem");

        if (planetSystem != null)
        {
            Object.ModelController.SetString("furniture_planetsystem_data", planetSystem.ToString());
        }
    }
}
