// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/PartSetsData.as

using System.Xml.Linq;

using Vortex.Habbo.Avatar.Actions;
using Vortex.Habbo.Avatar.Structure.Parts;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/PartSetsData.as
public class PartSetsData : IStructureData
{
    /// @see PartSetsData.as::PartSetsData
    public PartSetsData()
    {
        Parts = new Dictionary<string, PartDefinition>();
        ActivePartSets = new Dictionary<string, ActivePartSet>();
    }

    /// @see PartSetsData.as::parse
    public bool Parse(XElement xml)
    {
        if (xml == null)
        {
            return false;
        }

        // CRITICAL: AS3 reads param1.partSet[0].part — first partSet element only
        XElement? firstPartSet = xml.Element("partSet");
        if (firstPartSet != null)
        {
            foreach (XElement partXml in firstPartSet.Elements("part"))
            {
                // PRODUCTION source: _local_2.@["set-type"] — ATTRIBUTE, not child element
                string setType = partXml.Attribute("set-type")?.Value ?? "";
                Parts[setType] = new PartDefinition(partXml);
            }
        }

        foreach (XElement activePartSetXml in xml.Elements("activePartSet"))
        {
            string id = activePartSetXml.Attribute("id")?.Value ?? "";
            ActivePartSets[id] = new ActivePartSet(activePartSetXml);
        }

        return true;
    }

    /// @see PartSetsData.as::appendXML
    public bool AppendXml(XElement xml)
    {
        if (xml == null)
        {
            return false;
        }

        // Same as parse — first partSet element only
        XElement? firstPartSet = xml.Element("partSet");
        if (firstPartSet != null)
        {
            foreach (XElement partXml in firstPartSet.Elements("part"))
            {
                // PRODUCTION source: _local_2.@["set-type"] — ATTRIBUTE, not child element
                string setType = partXml.Attribute("set-type")?.Value ?? "";
                Parts[setType] = new PartDefinition(partXml);
            }
        }

        foreach (XElement activePartSetXml in xml.Elements("activePartSet"))
        {
            string id = activePartSetXml.Attribute("id")?.Value ?? "";
            ActivePartSets[id] = new ActivePartSet(activePartSetXml);
        }

        // AS3 always returns false
        return false;
    }

    /// @see PartSetsData.as::getActiveParts
    public List<string> GetActiveParts(IActionDefinition definition)
    {
        if (ActivePartSets.TryGetValue(definition.ActivePartSet, out ActivePartSet? activePartSet))
        {
            return activePartSet.Parts;
        }
        return new List<string>();
    }

    /// @see PartSetsData.as::getPartDefinition
    public PartDefinition? GetPartDefinition(string setType)
    {
        Parts.TryGetValue(setType, out PartDefinition? part);
        return part;
    }

    /// @see PartSetsData.as::addPartDefinition
    public PartDefinition AddPartDefinition(XElement xml)
    {
        // PRODUCTION source: k.@["set-type"] — ATTRIBUTE, not child element
        string setType = xml.Attribute("set-type")?.Value ?? "";
        if (!Parts.ContainsKey(setType))
        {
            Parts[setType] = new PartDefinition(xml);
        }
        return Parts[setType];
    }

    /// @see PartSetsData.as::get parts
    public Dictionary<string, PartDefinition> Parts { get; }

    /// @see PartSetsData.as::get activePartSets
    public Dictionary<string, ActivePartSet> ActivePartSets { get; }

    /// @see PartSetsData.as::getActivePartSet
    public ActivePartSet? GetActivePartSet(IActionDefinition definition)
    {
        ActivePartSets.TryGetValue(definition.ActivePartSet, out ActivePartSet? activePartSet);
        return activePartSet;
    }
}
