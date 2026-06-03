// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/Palette.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/Palette.as
public class Palette : IPalette
{
    private readonly Dictionary<int, IPartColor> _colors;

    /// @see Palette.as::Palette
    public Palette(XElement xml)
    {
        Id = int.Parse(xml.Attribute("id")?.Value ?? "0");
        _colors = new Dictionary<int, IPartColor>();
        Append(xml);
    }

    /// @see Palette.as::append
    public void Append(XElement xml)
    {
        foreach (XElement colorXml in xml.Elements("color"))
        {
            int colorId = int.Parse(colorXml.Attribute("id")?.Value ?? "0");
            _colors[colorId] = new PartColor(colorXml);
        }
    }

    /// @see Palette.as::get id
    public int Id { get; }

    /// @see Palette.as::getColor
    public IPartColor? GetColor(int id)
    {
        _colors.TryGetValue(id, out IPartColor? color);
        return color;
    }

    /// @see Palette.as::get colors
    public IDictionary<int, IPartColor> Colors => _colors;
}
