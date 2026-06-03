// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/GeometryBodyPart.as

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/GeometryBodyPart.as
public class GeometryBodyPart : AvatarNode3D
{
    private readonly Dictionary<string, GeometryItem> _staticItems;
    private readonly ConditionalWeakTable<IAvatarImage, Dictionary<string, GeometryItem>> _dynamicItems;

    /// @see GeometryBodyPart.as::GeometryBodyPart
    public GeometryBodyPart(XElement xml)
        : base(
            double.Parse(xml.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
            double.Parse(xml.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
            double.Parse(xml.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
        )
    {
        Radius = double.Parse(xml.Attribute("radius")?.Value ?? "0", CultureInfo.InvariantCulture);
        Id = xml.Attribute("id")?.Value ?? "";
        _staticItems = new Dictionary<string, GeometryItem>();
        _dynamicItems = new ConditionalWeakTable<IAvatarImage, Dictionary<string, GeometryItem>>();

        // AS3: param1..item — all descendant <item> elements
        foreach (XElement itemXml in xml.Descendants("item"))
        {
            GeometryItem item = new(itemXml);
            _staticItems[itemXml.Attribute("id")?.Value ?? ""] = item;
        }
    }

    /// @see GeometryBodyPart.as::getDynamicParts
    public List<GeometryItem> GetDynamicParts(IAvatarImage avatarImage)
    {
        List<GeometryItem> result = new();

        if (_dynamicItems.TryGetValue(avatarImage, out Dictionary<string, GeometryItem>? items))
        {
            result.AddRange(items.Values.OfType<GeometryItem>());
        }

        return result;
    }

    /// @see GeometryBodyPart.as::getPartIds
    public List<string> GetPartIds(IAvatarImage? avatarImage)
    {
        List<string> result = _staticItems.Values.OfType<GeometryItem>().Select(item => item.Id).ToList();

        if (avatarImage != null && _dynamicItems.TryGetValue(avatarImage, out Dictionary<string, GeometryItem>? dynamicDict))
        {
            result.AddRange(dynamicDict.Values.OfType<GeometryItem>().Select(item => item.Id));
        }

        return result;
    }

    /// @see GeometryBodyPart.as::removeDynamicParts
    public bool RemoveDynamicParts(IAvatarImage avatarImage)
    {
        _dynamicItems.Remove(avatarImage);
        return true;
    }

    /// @see GeometryBodyPart.as::addPart
    public bool AddPart(XElement xml, IAvatarImage avatarImage)
    {
        string id = xml.Attribute("id")?.Value ?? "";

        if (HasPart(id, avatarImage))
        {
            return false;
        }

        if (!_dynamicItems.TryGetValue(avatarImage, out Dictionary<string, GeometryItem>? dynamicDict))
        {
            dynamicDict = new Dictionary<string, GeometryItem>();
            _dynamicItems.AddOrUpdate(avatarImage, dynamicDict);
        }

        dynamicDict[id] = new GeometryItem(xml, true);
        return true;
    }

    /// @see GeometryBodyPart.as::hasPart
    public bool HasPart(string id, IAvatarImage? avatarImage)
    {
        if (_staticItems.TryGetValue(id, out GeometryItem? staticItem) && staticItem != null)
        {
            return true;
        }

        return avatarImage != null
               && _dynamicItems.TryGetValue(avatarImage, out Dictionary<string, GeometryItem>? dynamicDict)
               && dynamicDict.TryGetValue(id, out GeometryItem? dynamicItem)
               && dynamicItem != null;

    }

    /// @see GeometryBodyPart.as::getParts
    public List<string> GetParts(AvatarMatrix4x4 matrix, AvatarVector3D camera, object? param3, IAvatarImage? avatarImage)
    {
        List<(double Distance, GeometryItem Item)> distancePairs = new();

        foreach (GeometryItem item in _staticItems.Values.OfType<GeometryItem>())
        {
            item.ApplyTransform(matrix);

            double distance = item.GetDistance(camera);

            distancePairs.Add((distance, item));
        }

        if (avatarImage != null && _dynamicItems.TryGetValue(avatarImage, out Dictionary<string, GeometryItem>? dynamicDict))
        {
            foreach (GeometryItem item in dynamicDict.Values.OfType<GeometryItem>())
            {
                item.ApplyTransform(matrix);

                double distance = item.GetDistance(camera);

                distancePairs.Add((distance, item));
            }
        }

        // AS3: _loc9_.sort(orderParts) — ascending by distance
        distancePairs.Sort(OrderParts);

        List<string> result = new();

        foreach ((double _, GeometryItem item) in distancePairs)
        {
            result.Add(item.Id);
        }

        return result;
    }

    /// @see GeometryBodyPart.as::getDistance
    public double GetDistance(AvatarVector3D camera)
    {
        double d1 = Math.Abs(camera.Z - TransformedLocation.Z - Radius);
        double d2 = Math.Abs(camera.Z - TransformedLocation.Z + Radius);

        return Math.Min(d1, d2);
    }

    /// @see GeometryBodyPart.as::get id
    public string Id { get; }

    /// @see GeometryBodyPart.as::get radius
    public double Radius { get; }

    /// @see GeometryBodyPart.as::orderParts
    private static int OrderParts((double Distance, GeometryItem Item) a, (double Distance, GeometryItem Item) b)
    {
        if (a.Distance < b.Distance)
        {
            return -1;
        }

        if (a.Distance > b.Distance)
        {
            return 1;
        }

        return 0;
    }
}
