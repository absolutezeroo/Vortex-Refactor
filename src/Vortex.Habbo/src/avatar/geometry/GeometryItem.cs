// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/GeometryItem.as

using System;
using System.Globalization;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Geometry;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/geometry/GeometryItem.as
public class GeometryItem : AvatarNode3D
{
    private readonly double _radius;

    /// @see GeometryItem.as::GeometryItem
    public GeometryItem(XElement xml, bool isDynamic = false)
        : base(
            double.Parse(xml.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture),
            double.Parse(xml.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture),
            double.Parse(xml.Attribute("z")?.Value ?? "0", CultureInfo.InvariantCulture)
        )
    {
        Id = xml.Attribute("id")?.Value ?? "";
        _radius = double.Parse(xml.Attribute("radius")?.Value ?? "0", CultureInfo.InvariantCulture);
        Normal = new AvatarVector3D(
            double.Parse(xml.Attribute("nx")?.Value ?? "0", CultureInfo.InvariantCulture),
            double.Parse(xml.Attribute("ny")?.Value ?? "0", CultureInfo.InvariantCulture),
            double.Parse(xml.Attribute("nz")?.Value ?? "0", CultureInfo.InvariantCulture)
        );

        // AS3: parseInt(param1.@double) as Boolean — truthy int cast
        IsDoubleSided = int.TryParse(xml.Attribute("double")?.Value, out int doubleSided) && doubleSided != 0;
        IsDynamic = isDynamic;
    }

    /// @see GeometryItem.as::getDistance
    public double GetDistance(AvatarVector3D camera)
    {
        double d1 = Math.Abs(camera.Z - TransformedLocation.Z - _radius);
        double d2 = Math.Abs(camera.Z - TransformedLocation.Z + _radius);

        return Math.Min(d1, d2);
    }

    /// @see GeometryItem.as::get id
    public string Id { get; }

    /// @see GeometryItem.as::get normal
    public AvatarVector3D Normal { get; }

    /// @see GeometryItem.as::get isDoubleSided
    public bool IsDoubleSided { get; }

    /// @see GeometryItem.as::get isDynamic
    public bool IsDynamic { get; }

    /// @see GeometryItem.as::toString
    public override string ToString()
    {
        return Id + ": " + Location + " - " + TransformedLocation;
    }
}
