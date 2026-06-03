// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/class_3617.as

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/class_3617.as
public interface IFigurePart
{
    /// @see class_3617.as::get id
    int Id { get; }

    /// @see class_3617.as::get type
    string Type { get; }

    /// @see class_3617.as::get breed
    int Breed { get; }

    /// @see class_3617.as::get colorLayerIndex
    int ColorLayerIndex { get; }

    /// @see class_3617.as::get index
    int Index { get; }

    /// @see class_3617.as::get paletteMap
    int PaletteMap { get; }
}
