namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

using Vortex.Habbo.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureExternalImageLogic
public class FurnitureExternalImageLogic : FurnitureMultiStateLogic
{
    public override string? Widget => "RWE_EXTERNAL_IMAGE";

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.STICKIE,
            RoomObjectFurnitureActionEvent.STICKIE,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(System.Xml.Linq.XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        XElement? mask = xml.Element("mask");

        if (mask == null)
        {
            return;
        }

        string? maskType = (string?)mask.Attribute("type");

        if (maskType == null)
        {
            return;
        }

        Object.ModelController.SetNumber("furniture_uses_plane_mask", 1);
        Object.ModelController.SetString("furniture_plane_mask_type", maskType);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.STICKIE, Object));
    }
}
