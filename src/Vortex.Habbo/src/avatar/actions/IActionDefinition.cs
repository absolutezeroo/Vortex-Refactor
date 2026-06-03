// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/class_3576.as

namespace Vortex.Habbo.Avatar.Actions;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/class_3576.as
public interface IActionDefinition
{
    /// @see class_3576.as::get id
    string Id { get; }

    /// @see class_3576.as::get state
    string State { get; }

    /// @see class_3576.as::get precedence
    int Precedence { get; }

    /// @see class_3576.as::get activePartSet
    string ActivePartSet { get; }

    /// @see class_3576.as::get isMain
    bool IsMain { get; }

    /// @see class_3576.as::get isDefault
    bool IsDefault { get; }

    /// @see class_3576.as::get assetPartDefinition
    string AssetPartDefinition { get; }

    /// @see class_3576.as::get lay
    string Lay { get; }

    /// @see class_3576.as::get geometryType
    string GeometryType { get; }

    /// @see class_3576.as::get isAnimation
    bool IsAnimation { get; }

    /// @see class_3576.as::get startFromFrameZero
    bool StartFromFrameZero { get; }

    /// @see class_3576.as::isAnimated
    bool IsAnimated(string param1);

    /// @see class_3576.as::getPrevents
    string[] GetPrevents(string param1 = "");

    /// @see class_3576.as::getPreventHeadTurn
    bool GetPreventHeadTurn(string param1 = "");

    /// @see class_3576.as::setOffsets
    void SetOffsets(string param1, int param2, double[] param3);

    /// @see class_3576.as::getOffsets
    double[]? GetOffsets(string param1, int param2);
}
