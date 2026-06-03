// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/parts/ActivePartSet.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Parts;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/parts/ActivePartSet.as
public class ActivePartSet
{
    private readonly string _id;

    /// @see ActivePartSet.as::ActivePartSet
    public ActivePartSet(XElement xml)
    {
        _id = xml.Attribute("id")?.Value ?? "";
        Parts = new List<string>();

        foreach (XElement activePart in xml.Elements("activePart"))
        {
            // AS3: _loc2_.@["set-type"] — attribute access (confirmed via PRODUCTION-2016 deobfuscated source)
            Parts.Add(activePart.Attribute("set-type")?.Value ?? "");
        }
    }

    /// @see ActivePartSet.as::get parts
    public List<string> Parts { get; }
}
