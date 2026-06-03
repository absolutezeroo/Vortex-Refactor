// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/class_3360.as

using Vortex.Habbo.Avatar.Structure.Figure;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/class_3360.as
public interface IFigureData
{
    /// @see class_3360.as::getSetType
    ISetType? GetSetType(string param1);

    /// @see class_3360.as::getPalette
    IPalette? GetPalette(int param1);

    /// @see class_3360.as::getFigurePartSet
    IFigurePartSet? GetFigurePartSet(int param1);
}
