// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3405.as

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3405.as
public interface IFigureContainer
{
    /// @see class_3405.as::getPartTypeIds
    string[] GetPartTypeIds();

    /// @see class_3405.as::hasPartType
    bool HasPartType(string param1);

    /// @see class_3405.as::getPartSetId
    int GetPartSetId(string param1);

    /// @see class_3405.as::getPartColorIds
    int[] GetPartColorIds(string param1);

    /// @see class_3405.as::updatePart
    void UpdatePart(string param1, int param2, int[] param3);

    /// @see class_3405.as::removePart
    void RemovePart(string param1);

    /// @see class_3405.as::getFigureString
    string GetFigureString();
}
