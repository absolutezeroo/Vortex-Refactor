// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/ActiveActionData.as
// @see PRODUCTION-201611291003-338511768-Source-main/src/com/sulake/habbo/avatar/actions/ActiveActionData.as

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Avatar.Actions;

/// <summary>
/// Holds runtime data for a single active action on an avatar.
/// Links action type/parameter to its resolved definition.
/// </summary>
/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/ActiveActionData.as
public class ActiveActionData : IActiveActionData, IDisposable
{
    private string? _actionType = "";
    private string? _actionParameter = "";

    /// @see ActiveActionData.as::ActiveActionData
    public ActiveActionData(string actionType, string actionParameter = "", int startFrame = 0)
    {
        _actionType = actionType;
        _actionParameter = actionParameter;
        StartFrame = startFrame;
    }

    /// @see ActiveActionData.as::get id
    public string Id
    {
        get
        {
            if (Definition == null)
            {
                return "";
            }
            return Definition.Id + "_" + _actionParameter;
        }
    }

    /// @see ActiveActionData.as::get actionType
    public string ActionType => _actionType ?? "";

    /// @see ActiveActionData.as::get actionParameter
    /// @see ActiveActionData.as::set actionParameter
    public string ActionParameter
    {
        get => _actionParameter ?? "";
        set => _actionParameter = value;
    }

    /// @see ActiveActionData.as::get definition
    /// @see ActiveActionData.as::set definition
    public IActionDefinition? Definition { get; set; }

    /// @see ActiveActionData.as::get startFrame
    public int StartFrame { get; }

    /// @see ActiveActionData.as::get overridingAction
    /// @see ActiveActionData.as::set overridingAction
    public string? OverridingAction { get; set; }

    /// @see ActiveActionData.as::dispose
    public void Dispose()
    {
        _actionType = null;
        _actionParameter = null;
        Definition = null;
    }

    /// @see IDisposable::get disposed
    public bool disposed => _actionType == null;

    /// @see ActiveActionData.as::toString
    public override string ToString()
    {
        return "Action: " + _actionType + "  param: " + _actionParameter;
    }
}
