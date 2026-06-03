// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/class_3445.as

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/class_3445.as
public interface IPalette
{
    /// @see class_3445.as::get id
    int Id { get; }

    /// @see class_3445.as::getColor
    IPartColor? GetColor(int param1);

    /// @see class_3445.as::get colors
    IDictionary<int, IPartColor> Colors { get; }
}
