// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/FigurePartSet.as

using System.Linq;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/FigurePartSet.as
public class FigurePartSet : IFigurePartSet
{
    private readonly List<IFigurePart> _parts;
    private readonly List<string> _hiddenLayers;

    /// @see FigurePartSet.as::FigurePartSet
    public FigurePartSet(XElement xml, string type)
    {
        Type = type;
        Id = int.Parse(xml.Attribute("id")?.Value ?? "0");
        Gender = xml.Attribute("gender")?.Value ?? "";
        ClubLevel = int.Parse(xml.Attribute("club")?.Value ?? "0");

        string colorableStr = xml.Attribute("colorable")?.Value ?? "0";
        IsColorable = int.TryParse(colorableStr, out int colorVal) && colorVal != 0;

        string selectableStr = xml.Attribute("selectable")?.Value ?? "0";
        IsSelectable = int.TryParse(selectableStr, out int selVal) && selVal != 0;

        string preSelectableStr = xml.Attribute("preselectable")?.Value ?? "0";
        IsPreSelectable = int.TryParse(preSelectableStr, out int preSelVal) && preSelVal != 0;

        string sellableStr = xml.Attribute("sellable")?.Value ?? "0";
        IsSellable = int.TryParse(sellableStr, out int sellVal) && sellVal != 0;

        _parts = new List<IFigurePart>();
        _hiddenLayers = new List<string>();

        foreach (XElement partXml in xml.Elements("part"))
        {
            FigurePart figurePart = new(partXml);
            int insertIndex = IndexOfPartType(figurePart);

            if (insertIndex != -1)
            {
                _parts.Insert(insertIndex, figurePart);
            }
            else
            {
                _parts.Add(figurePart);
            }
        }

        foreach (XElement layerXml in xml.Elements("hiddenlayers").Elements("layer"))
        {
            _hiddenLayers.Add(layerXml.Attribute("parttype")?.Value ?? "");
        }
    }

    /// @see FigurePartSet.as::indexOfPartType
    private int IndexOfPartType(FigurePart part)
    {
        for (int i = 0;
             i < _parts.Count;
             i++)
        {
            IFigurePart existing = _parts[i];

            if (existing.Type == part.Type && existing.Index < part.Index)
            {
                return i;
            }
        }
        return -1;
    }

    /// @see FigurePartSet.as::getPart
    public IFigurePart? GetPart(string type, int id)
    {
        return _parts.FirstOrDefault(part => part.Type == type && part.Id == id);
    }

    /// @see FigurePartSet.as::get type
    public string Type { get; }

    /// @see FigurePartSet.as::get id
    public int Id { get; }

    /// @see FigurePartSet.as::get gender
    public string Gender { get; }

    /// @see FigurePartSet.as::get clubLevel
    public int ClubLevel { get; }

    /// @see FigurePartSet.as::get isColorable
    public bool IsColorable { get; }

    /// @see FigurePartSet.as::get isSelectable
    public bool IsSelectable { get; }

    /// @see FigurePartSet.as::get isPreSelectable
    public bool IsPreSelectable { get; }

    /// @see FigurePartSet.as::get isSellable
    public bool IsSellable { get; }

    /// @see FigurePartSet.as::get parts
    public IList<IFigurePart> Parts => _parts;

    /// @see FigurePartSet.as::get hiddenLayers
    public IList<string> HiddenLayers => _hiddenLayers;
}
