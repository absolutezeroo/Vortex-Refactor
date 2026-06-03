namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureCreditLogic
public class FurnitureCreditLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.CREDITFURNI];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        string? credits = (string?)xml.Element("credits");

        if (credits != null && int.TryParse(credits, out int value))
        {
            Object.ModelController.SetNumber("furniture_credit_value", value);
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.CREDITFURNI, Object));

        base.UseObject();
    }
}
