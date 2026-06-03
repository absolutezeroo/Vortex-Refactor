namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureWindowLogic (class_3435)
public class FurnitureWindowLogic : FurnitureMultiStateLogic
{
    public override void Initialize(XElement? xml)
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
}
