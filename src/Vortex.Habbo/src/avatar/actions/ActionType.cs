// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/ActionType.as
// @see PRODUCTION-201611291003-338511768-Source-main/src/com/sulake/habbo/avatar/actions/ActionType.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Actions;

/// <summary>
/// Describes a sub-type within an action definition.
/// Each type overrides the parent action's prevents, preventHeadTurn, and isAnimated values.
/// </summary>
/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/ActionType.as
public class ActionType
{
    /// @see ActionType.as::ActionType
    public ActionType(XElement xml)
    {
        Id = int.TryParse((string?)xml.Attribute("value"), out int idVal) ? idVal : 0;
        Value = Id;

        string preventsStr = (string?)xml.Attribute("prevents") ?? "";

        if (preventsStr != "")
        {
            Prevents = preventsStr.Split(',');
        }
        else
        {
            Prevents = [];
        }

        PreventHeadTurn = (string?)xml.Attribute("preventheadturn") == "true";

        string animatedStr = (string?)xml.Attribute("animated") ?? "";

        if (animatedStr == "")
        {
            IsAnimated = true;
        }
        else
        {
            IsAnimated = animatedStr == "true";
        }
    }

    /// @see ActionType.as::get id
    public int Id { get; }

    /// @see ActionType.as::get value
    public int Value { get; }

    /// @see ActionType.as::get prevents
    public string[] Prevents { get; }

    /// @see ActionType.as::get preventHeadTurn
    public bool PreventHeadTurn { get; }

    /// @see ActionType.as::get isAnimated
    public bool IsAnimated { get; }
}
