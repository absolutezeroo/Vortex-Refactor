// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/parts/PartDefinition.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Parts;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/parts/PartDefinition.as
public class PartDefinition
{
    /// @see PartDefinition.as::PartDefinition
    public PartDefinition(XElement xml)
    {
        // PRODUCTION source: k.@["set-type"] — reads ATTRIBUTES (obfuscated WIN63 dropped the @)
        SetType = xml.Attribute("set-type")?.Value ?? "";
        FlippedSetType = xml.Attribute("flipped-set-type")?.Value ?? "";
        RemoveSetType = xml.Attribute("remove-set-type")?.Value ?? "";
        AppendToFigure = false;
        StaticId = -1;
    }

    /// @see PartDefinition.as::hasStaticId
    public bool HasStaticId()
    {
        return StaticId >= 0;
    }

    /// @see PartDefinition.as::get staticId
    /// @see PartDefinition.as::set staticId
    public int StaticId { get; set; }

    /// @see PartDefinition.as::get setType
    public string SetType { get; }

    /// @see PartDefinition.as::get flippedSetType
    /// @see PartDefinition.as::set flippedSetType
    public string FlippedSetType { get; set; }

    /// @see PartDefinition.as::get removeSetType
    public string RemoveSetType { get; }

    /// @see PartDefinition.as::get appendToFigure
    /// @see PartDefinition.as::set appendToFigure
    public bool AppendToFigure { get; set; }
}
