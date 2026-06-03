// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/DirectionDataContainer.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/DirectionDataContainer.as
public class DirectionDataContainer
{
    /// @see DirectionDataContainer.as::DirectionDataContainer
    public DirectionDataContainer(XElement xml)
    {
        Offset = (int?)xml.Attribute("offset") ?? 0;
    }

    /// @see DirectionDataContainer.as::get offset
    public int Offset { get; }
}
