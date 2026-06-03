// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/animation/AnimationFrame.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Structure.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/animation/AnimationFrame.as
public class AnimationFrame
{
    /// @see AnimationFrame.as::AnimationFrame
    public AnimationFrame(XElement xml)
    {
        Number = int.Parse(xml.Attribute("number")?.Value ?? "0");
        AssetPartDefinition = xml.Attribute("assetpartdefinition")?.Value ?? "";
    }

    /// @see AnimationFrame.as::get number
    public int Number { get; }

    /// @see AnimationFrame.as::get assetPartDefinition
    public string AssetPartDefinition { get; }
}
