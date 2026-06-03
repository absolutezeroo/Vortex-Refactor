// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/FigurePart.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/FigurePart.as
public class FigurePart : IFigurePart
{
    /// @see FigurePart.as::FigurePart
    public FigurePart(XElement xml)
    {
        Id = int.Parse(xml.Attribute("id")?.Value ?? "0");
        Type = xml.Attribute("type")?.Value ?? "";
        Index = int.Parse(xml.Attribute("index")?.Value ?? "0");
        ColorLayerIndex = int.Parse(xml.Attribute("colorindex")?.Value ?? "0");

        PaletteMap = -1;
        string paletteMapStr = xml.Attribute("palettemapid")?.Value ?? "";

        if (paletteMapStr != "")
        {
            PaletteMap = int.Parse(paletteMapStr);
        }

        Breed = -1;
        string breedStr = xml.Attribute("breed")?.Value ?? "";

        if (breedStr != "")
        {
            Breed = int.Parse(breedStr);
        }
    }

    /// @see FigurePart.as::get id
    public int Id { get; }

    /// @see FigurePart.as::get type
    public string Type { get; }

    /// @see FigurePart.as::get breed
    public int Breed { get; }

    /// @see FigurePart.as::get colorLayerIndex
    public int ColorLayerIndex { get; }

    /// @see FigurePart.as::get index
    public int Index { get; }

    /// @see FigurePart.as::get paletteMap
    public int PaletteMap { get; }
}
