// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/AvatarActionManager.as
// @see PRODUCTION-201611291003-338511768-Source-main/src/com/sulake/habbo/avatar/actions/AvatarActionManager.as

using System.Linq;
using System.Xml.Linq;

using Vortex.Core.Assets;

namespace Vortex.Habbo.Avatar.Actions;

/// <summary>
/// Manages avatar action definitions parsed from XML.
/// Resolves action offsets from assets, filters/sorts active actions by precedence,
/// and provides lookups by id or state.
/// </summary>
/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/actions/AvatarActionManager.as
public class AvatarActionManager
{
    private readonly IAssetLibrary _assets;
    private readonly Dictionary<string, ActionDefinition> _actions;
    private ActionDefinition? _defaultAction;

    /// @see AvatarActionManager.as::AvatarActionManager
    public AvatarActionManager(IAssetLibrary assets, XElement actionsXml)
    {
        _assets = assets;
        _actions = new Dictionary<string, ActionDefinition>();
        UpdateActions(actionsXml);
    }

    /// @see AvatarActionManager.as::updateActions (PRODUCTION: _Str_1620)
    public void UpdateActions(XElement xml)
    {
        _defaultAction = null;

        foreach (XElement actionEl in xml.Elements("action"))
        {
            string state = (string?)actionEl.Attribute("state") ?? "";

            if (state == "")
            {
                continue;
            }

            ActionDefinition definition = new(actionEl);
            _actions[state] = definition;
        }

        ParseActionOffsets();
    }

    /// @see AvatarActionManager.as::parseActionOffsets
    private void ParseActionOffsets()
    {
        foreach (ActionDefinition action in _actions.Values)
        {
            string state = action.State;

            if (!_assets.HasAsset("action_offset_" + state))
            {
                continue;
            }

            IAsset? asset = _assets.GetAssetByName("action_offset_" + state);

            if (asset?.Content is not XElement xml)
            {
                continue;
            }

            foreach (XElement offsetEl in xml.Elements("offset"))
            {
                string size = (string?)offsetEl.Attribute("size") ?? "";
                int direction = int.TryParse((string?)offsetEl.Attribute("direction"), out int dir) ? dir : 0;
                int x = int.TryParse((string?)offsetEl.Attribute("x"), out int xVal) ? xVal : 0;
                int y = int.TryParse((string?)offsetEl.Attribute("y"), out int yVal) ? yVal : 0;
                double z = double.TryParse((string?)offsetEl.Attribute("z"), out double zVal) ? zVal : double.NaN;

                action.SetOffsets(size, direction, [x, y, z]);
            }
        }
    }

    /// @see AvatarActionManager.as::getActionDefinition (PRODUCTION: _Str_1675)
    public ActionDefinition? GetActionDefinition(string id)
    {
        return _actions.Values.FirstOrDefault(action => action.Id == id);

    }

    /// @see AvatarActionManager.as::getActionDefinitionWithState (PRODUCTION: _Str_2018)
    public ActionDefinition? GetActionDefinitionWithState(string state)
    {
        return _actions.GetValueOrDefault(state);
    }

    /// @see AvatarActionManager.as::getDefaultAction
    public ActionDefinition? GetDefaultAction()
    {
        if (_defaultAction != null)
        {
            return _defaultAction;
        }

        foreach (ActionDefinition action in _actions.Values.Where(action => action.IsDefault))
        {
            _defaultAction = action;

            return action;
        }

        return null;
    }

    /// @see AvatarActionManager.as::getCanvasOffsets (PRODUCTION: _Str_781)
    public double[]? GetCanvasOffsets(IList<IActiveActionData> actions, string scale, int direction)
    {
        double[]? result = null;

        foreach (IActiveActionData activeAction in actions)
        {
            if (!_actions.TryGetValue(activeAction.ActionType, out ActionDefinition? definition))
            {
                continue;
            }

            double[]? offsets = definition.GetOffsets(scale, direction);

            if (offsets != null)
            {
                result = offsets;
            }
        }

        return result;
    }

    /// @see AvatarActionManager.as::sortActions (PRODUCTION: isHeadTurnPreventedByAction)
    public IList<IActiveActionData> SortActions(IList<IActiveActionData> actions)
    {
        actions = FilterActions(actions);
        List<IActiveActionData> result = new();

        foreach (IActiveActionData activeAction in actions)
        {
            if (!_actions.TryGetValue(activeAction.ActionType, out ActionDefinition? definition))
            {
                continue;
            }

            activeAction.Definition = definition;

            result.Add(activeAction);
        }

        result.Sort(OrderByPrecedence);
        return result;
    }

    /// @see AvatarActionManager.as::filterActions (PRODUCTION: _Str_1247)
    private IList<IActiveActionData> FilterActions(IList<IActiveActionData> actions)
    {
        // Pass 1: Collect all prevents from all actions
        List<string> prevents = new();

        foreach (IActiveActionData activeAction in actions)
        {
            if (_actions.TryGetValue(activeAction.ActionType, out ActionDefinition? definition))
            {
                prevents.AddRange(definition.GetPrevents(activeAction.ActionParameter));
            }
        }

        // Pass 2: Filter out prevented actions
        List<IActiveActionData> result = new();

        foreach (IActiveActionData activeAction in actions)
        {
            string key = activeAction.ActionType;

            // fx actions use "fx.param" as their prevent key
            if (activeAction.ActionType == AvatarAction.EFFECT)
            {
                key += "." + activeAction.ActionParameter;
            }

            if (!prevents.Contains(key))
            {
                result.Add(activeAction);
            }
        }

        return result;
    }

    /// @see AvatarActionManager.as::orderByPrecedence
    /// Higher precedence comes first (descending sort).
    private static int OrderByPrecedence(IActiveActionData a, IActiveActionData b)
    {
        int precA = a.Definition?.Precedence ?? 0;
        int precB = b.Definition?.Precedence ?? 0;

        if (precA < precB)
        {
            return 1;
        }

        if (precA > precB)
        {
            return -1;
        }

        return 0;
    }
}
