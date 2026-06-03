using System.Globalization;
using System.Xml.Linq;

using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object;

/// <summary>
/// Manages bitmap masks overlaid on room planes. Parses from XML and provides
/// add/remove/query operations for plane masks with location, type and category.
/// </summary>
/// @see com.sulake.habbo.room.object.RoomPlaneBitmapMaskParser
public class RoomPlaneBitmapMaskParser
{
    private List<(string Key, RoomPlaneBitmapMaskData Data)>? _masks = new();

    public int MaskCount => _masks?.Count ?? 0;

    public void Dispose()
    {
        if (_masks == null)
        {
            return;
        }

        Reset();

        _masks = null;
    }

    public bool Initialize(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        _masks!.Clear();

        string[] requiredMask = ["id", "type", "category"];
        string[] requiredLoc = ["x", "y", "z"];

        foreach (XElement maskElement in xml.Elements("planeMask"))
        {
            if (!XMLValidator.CheckRequiredAttributes(maskElement, requiredMask))
            {
                return false;
            }

            string id = maskElement.Attribute("id")!.Value;
            string type = maskElement.Attribute("type")!.Value;
            string category = maskElement.Attribute("category")!.Value;

            IEnumerable<XElement> locationElements = maskElement.Elements("location");
            XElement? locElement = null;
            int count = 0;

            foreach (XElement le in locationElements)
            {
                locElement = le;
                count++;
            }

            if (count != 1)
            {
                return false;
            }

            if (!XMLValidator.CheckRequiredAttributes(locElement!, requiredLoc))
            {
                return false;
            }

            double x = double.Parse(locElement!.Attribute("x")!.Value, CultureInfo.InvariantCulture);
            double y = double.Parse(locElement.Attribute("y")!.Value, CultureInfo.InvariantCulture);
            double z = double.Parse(locElement.Attribute("z")!.Value, CultureInfo.InvariantCulture);

            Vector3d loc = new(x, y, z);
            RoomPlaneBitmapMaskData maskData = new(type, loc, category);
            _masks.Add((id, maskData));
        }
        return true;
    }

    public void Reset()
    {
        if (_masks == null)
        {
            return;
        }

        foreach ((string _, RoomPlaneBitmapMaskData data) in _masks)
        {
            data.Dispose();
        }

        _masks.Clear();
    }

    public void AddMask(string id, string type, IVector3d location, string category)
    {
        RoomPlaneBitmapMaskData maskData = new(type, location, category);

        RemoveMask(id);
        _masks!.Add((id, maskData));
    }

    public bool RemoveMask(string id)
    {
        if (_masks == null)
        {
            return false;
        }

        for (int i = 0;
             i < _masks.Count;
             i++)
        {
            if (_masks[i].Key != id)
            {
                continue;
            }

            _masks[i].Data.Dispose();
            _masks.RemoveAt(i);

            return true;
        }

        return false;
    }

    public XElement GetXML()
    {
        XElement root = new("planeMasks");

        for (int i = 0;
             i < MaskCount;
             i++)
        {
            string? type = GetMaskType(i);
            string? category = GetMaskCategory(i);
            XElement maskElement = new(
                "planeMask",
                new XAttribute("id", i),
                new XAttribute("type", type ?? ""),
                new XAttribute("category", category ?? "")
            );

            IVector3d? loc = GetMaskLocation(i);

            if (loc == null)
            {
                continue;
            }

            maskElement.Add(
                new XElement(
                    "location",
                    new XAttribute("x", loc.X.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("y", loc.Y.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("z", loc.Z.ToString(CultureInfo.InvariantCulture))
                )
            );

            root.Add(maskElement);
        }

        return root;
    }

    public IVector3d? GetMaskLocation(int index)
    {
        if (index < 0 || index >= MaskCount)
        {
            return null;
        }

        return _masks![index].Data.Loc;
    }

    public string? GetMaskType(int index)
    {
        if (index < 0 || index >= MaskCount)
        {
            return null;
        }

        return _masks![index].Data.Type;
    }

    public string? GetMaskCategory(int index)
    {
        if (index < 0 || index >= MaskCount)
        {
            return null;
        }

        return _masks![index].Data.Category;
    }
}
