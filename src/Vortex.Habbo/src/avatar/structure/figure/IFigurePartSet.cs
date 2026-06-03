// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/class_3418.as

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/class_3418.as
public interface IFigurePartSet
{
    /// @see class_3418.as::getPart
    IFigurePart? GetPart(string param1, int param2);

    /// @see class_3418.as::get type
    string Type { get; }

    /// @see class_3418.as::get id
    int Id { get; }

    /// @see class_3418.as::get gender
    string Gender { get; }

    /// @see class_3418.as::get clubLevel
    int ClubLevel { get; }

    /// @see class_3418.as::get isColorable
    bool IsColorable { get; }

    /// @see class_3418.as::get isSelectable
    bool IsSelectable { get; }

    /// @see class_3418.as::get isPreSelectable
    bool IsPreSelectable { get; }

    /// @see class_3418.as::get isSellable
    bool IsSellable { get; }

    /// @see class_3418.as::get parts
    IList<IFigurePart> Parts { get; }

    /// @see class_3418.as::get hiddenLayers
    IList<string> HiddenLayers { get; }
}
