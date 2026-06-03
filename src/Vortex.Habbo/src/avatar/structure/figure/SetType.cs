// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/SetType.as

using System;
using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/SetType.as
public class SetType : ISetType
{
    private readonly Dictionary<string, bool[]> _mandatory;
    private readonly Dictionary<int, IFigurePartSet> _partSets;

    /// @see SetType.as::SetType
    public SetType(XElement xml)
    {
        Type = xml.Attribute("type")?.Value ?? "";
        PaletteId = int.Parse(xml.Attribute("paletteid")?.Value ?? "0");

        _mandatory = new Dictionary<string, bool[]>();

        string mandF0Str = xml.Attribute("mand_f_0")?.Value ?? "0";
        string mandF1Str = xml.Attribute("mand_f_1")?.Value ?? "0";
        string mandM0Str = xml.Attribute("mand_m_0")?.Value ?? "0";
        string mandM1Str = xml.Attribute("mand_m_1")?.Value ?? "0";

        _mandatory["F"] =
        [
            int.TryParse(mandF0Str, out int f0) && f0 != 0,
            int.TryParse(mandF1Str, out int f1) && f1 != 0,
        ];
        _mandatory["M"] =
        [
            int.TryParse(mandM0Str, out int m0) && m0 != 0,
            int.TryParse(mandM1Str, out int m1) && m1 != 0,
        ];

        _partSets = new Dictionary<int, IFigurePartSet>();

        Append(xml);
    }

    /// @see SetType.as::cleanUp
    public void CleanUp(XElement xml)
    {
        foreach (XElement setXml in xml.Elements("set"))
        {
            int setId = int.Parse(setXml.Attribute("id")?.Value ?? "0");

            if (_partSets.Remove(setId))
            {
                // AS3 disposes the removed part set; C# relies on GC
            }
        }
    }

    /// @see SetType.as::append
    public void Append(XElement xml)
    {
        foreach (XElement setXml in xml.Elements("set"))
        {
            int setId = int.Parse(setXml.Attribute("id")?.Value ?? "0");
            _partSets[setId] = new FigurePartSet(setXml, Type);
        }
    }

    /// @see SetType.as::getDefaultPartSet
    public IFigurePartSet? GetDefaultPartSet(string gender)
    {
        List<int> keys = new(_partSets.Keys);

        for (int i = keys.Count - 1;
             i >= 0;
             i--)
        {
            IFigurePartSet partSet = _partSets[keys[i]];

            if (partSet is { ClubLevel: 0 } && (partSet.Gender == gender || partSet.Gender == "U"))
            {
                return partSet;
            }
        }
        return null;
    }

    /// @see SetType.as::getPartSet
    public IFigurePartSet? GetPartSet(int id)
    {
        _partSets.TryGetValue(id, out IFigurePartSet? partSet);
        return partSet;
    }

    /// @see SetType.as::get type
    public string Type { get; }

    /// @see SetType.as::get paletteID
    public int PaletteId { get; }

    /// @see SetType.as::isMandatory
    public bool IsMandatory(string gender, int clubLevel)
    {
        string key = gender.ToUpperInvariant();

        return _mandatory.TryGetValue(key, out bool[]? flags) && flags[Math.Min(clubLevel, 1)];

    }

    /// @see SetType.as::optionalFromClubLevel
    public int OptionalFromClubLevel(string gender)
    {
        string key = gender.ToUpperInvariant();

        if (_mandatory.TryGetValue(key, out bool[]? flags))
        {
            return Array.IndexOf(flags, false);
        }

        return -1;
    }

    /// @see SetType.as::get partSets
    public IDictionary<int, IFigurePartSet> PartSets => _partSets;
}
