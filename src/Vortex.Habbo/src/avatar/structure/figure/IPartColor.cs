// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/IPartColor.as

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/IPartColor.as
public interface IPartColor
{
    /// @see IPartColor.as::get id
    int Id { get; }

    /// @see IPartColor.as::get index
    int Index { get; }

    /// @see IPartColor.as::get clubLevel
    int ClubLevel { get; }

    /// @see IPartColor.as::get isSelectable
    bool IsSelectable { get; }

    /// @see IPartColor.as::get redMultiplier
    double RedMultiplier { get; }

    /// @see IPartColor.as::get greenMultiplier
    double GreenMultiplier { get; }

    /// @see IPartColor.as::get blueMultiplier
    double BlueMultiplier { get; }

    /// @see IPartColor.as::get rgb
    uint Rgb { get; }

    /// @see IPartColor.as::get r
    uint R { get; }

    /// @see IPartColor.as::get g
    uint G { get; }

    /// @see IPartColor.as::get b
    uint B { get; }
}
