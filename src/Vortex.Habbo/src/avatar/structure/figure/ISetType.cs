// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/ISetType.as

namespace Vortex.Habbo.Avatar.Structure.Figure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/figure/ISetType.as
public interface ISetType
{
    /// @see ISetType.as::getPartSet
    IFigurePartSet? GetPartSet(int param1);

    /// @see ISetType.as::isMandatory
    bool IsMandatory(string param1, int param2);

    /// @see ISetType.as::optionalFromClubLevel
    int OptionalFromClubLevel(string param1);

    /// @see ISetType.as::get type
    string Type { get; }

    /// @see ISetType.as::get paletteID
    int PaletteId { get; }

    /// @see ISetType.as::get partSets
    IDictionary<int, IFigurePartSet> PartSets { get; }
}
