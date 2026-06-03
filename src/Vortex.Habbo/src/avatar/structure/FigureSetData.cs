// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/FigureSetData.as

using System.Linq;
using System.Xml.Linq;

using Vortex.Habbo.Avatar.Structure.Figure;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/FigureSetData.as
public class FigureSetData : IStructureData, IFigureData
{
    private readonly Dictionary<int, Palette> _palettes;
    private readonly Dictionary<string, SetType> _setTypes;

    /// @see FigureSetData.as::FigureSetData
    public FigureSetData()
    {
        _palettes = new Dictionary<int, Palette>();
        _setTypes = new Dictionary<string, SetType>();
    }

    /// @see FigureSetData.as::parse
    public bool Parse(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        foreach (XElement paletteXml in xml.Element("colors")?.Elements("palette") ?? [])
        {
            int paletteId = int.Parse(paletteXml.Attribute("id")?.Value ?? "0");
            _palettes[paletteId] = new Palette(paletteXml);
        }

        foreach (XElement setTypeXml in xml.Element("sets")?.Elements("settype") ?? [])
        {
            string type = setTypeXml.Attribute("type")?.Value ?? "";
            _setTypes[type] = new SetType(setTypeXml);
        }

        return true;
    }

    /// @see FigureSetData.as::injectXML
    public void InjectXml(XElement xml)
    {
        foreach (XElement setTypeXml in xml.Element("sets")?.Elements("settype") ?? [])
        {
            string type = setTypeXml.Attribute("type")?.Value ?? "";

            if (_setTypes.TryGetValue(type, out SetType? existing))
            {
                existing.CleanUp(setTypeXml);
            }
            else
            {
                _setTypes[type] = new SetType(setTypeXml);
            }
        }

        AppendXml(xml);
    }

    /// @see FigureSetData.as::appendXML
    public bool AppendXml(XElement? xml)
    {
        if (xml == null)
        {
            return false;
        }

        foreach (XElement paletteXml in xml.Element("colors")?.Elements("palette") ?? [])
        {
            int paletteId = int.Parse(paletteXml.Attribute("id")?.Value ?? "0");

            if (_palettes.TryGetValue(paletteId, out Palette? existing))
            {
                existing.Append(paletteXml);
            }
            else
            {
                _palettes[paletteId] = new Palette(paletteXml);
            }
        }

        foreach (XElement setTypeXml in xml.Element("sets")?.Elements("settype") ?? [])
        {
            string type = setTypeXml.Attribute("type")?.Value ?? "";

            if (_setTypes.TryGetValue(type, out SetType? existingSetType))
            {
                existingSetType.Append(setTypeXml);
            }
            else
            {
                _setTypes[type] = new SetType(setTypeXml);
            }
        }

        // AS3 always returns false
        return false;
    }

    /// @see FigureSetData.as::getMandatorySetTypeIds
    public List<string> GetMandatorySetTypeIds(string gender, int clubLevel)
    {
        return (from setType in _setTypes.Values where setType.IsMandatory(gender, clubLevel) select setType.Type).ToList();
    }

    /// @see FigureSetData.as::getDefaultPartSet
    public IFigurePartSet? GetDefaultPartSet(string type, string gender)
    {
        if (_setTypes.TryGetValue(type, out SetType? setType))
        {
            return setType.GetDefaultPartSet(gender);
        }

        return null;
    }

    /// @see FigureSetData.as::getSetType
    public ISetType? GetSetType(string type)
    {
        _setTypes.TryGetValue(type, out SetType? setType);

        return setType;
    }

    /// @see FigureSetData.as::getPalette
    public IPalette? GetPalette(int id)
    {
        _palettes.TryGetValue(id, out Palette? palette);

        return palette;
    }

    /// @see FigureSetData.as::getFigurePartSet
    public IFigurePartSet? GetFigurePartSet(int id)
    {
        return _setTypes.Values.Select(setType => setType.GetPartSet(id)).OfType<IFigurePartSet>().FirstOrDefault();
    }
}
