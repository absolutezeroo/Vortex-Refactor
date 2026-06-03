// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/class_3544.as

namespace Vortex.Habbo.Avatar.Actions;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/class_3544.as
public interface IActiveActionData
{
    /// @see class_3544.as::get id
    string Id { get; }

    /// @see class_3544.as::get actionType
    string ActionType { get; }

    /// @see class_3544.as::get actionParameter
    string ActionParameter { get; set; }

    /// @see class_3544.as::get startFrame
    int StartFrame { get; }

    /// @see class_3544.as::get definition
    IActionDefinition? Definition { get; set; }

    /// @see class_3544.as::get overridingAction
    string? OverridingAction { get; set; }
}
