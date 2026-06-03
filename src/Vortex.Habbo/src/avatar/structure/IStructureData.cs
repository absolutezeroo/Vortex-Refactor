// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/IStructureData.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/IStructureData.as
public interface IStructureData
{
    /// @see IStructureData.as::parse
    bool Parse(XElement param1);

    /// @see IStructureData.as::appendXML
    bool AppendXml(XElement param1);
}
