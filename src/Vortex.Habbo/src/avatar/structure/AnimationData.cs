// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/AnimationData.as

using System.Xml.Linq;

using Vortex.Habbo.Avatar.Actions;
using Vortex.Habbo.Avatar.Structure.Animation;

namespace Vortex.Habbo.Avatar.Structure;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/structure/AnimationData.as
public class AnimationData : IStructureData
{
    private readonly Dictionary<string, AnimationAction> _actions;

    /// @see AnimationData.as::AnimationData
    public AnimationData()
    {
        _actions = new Dictionary<string, AnimationAction>();
    }

    /// @see AnimationData.as::parse
    public bool Parse(XElement xml)
    {
        if (xml == null)
        {
            return false;
        }

        foreach (XElement actionXml in xml.Elements("action"))
        {
            string id = actionXml.Attribute("id")?.Value ?? "";
            _actions[id] = new AnimationAction(actionXml);
        }

        return true;
    }

    /// @see AnimationData.as::appendXML
    public bool AppendXml(XElement xml)
    {
        if (xml == null)
        {
            return false;
        }

        foreach (XElement actionXml in xml.Elements("action"))
        {
            string id = actionXml.Attribute("id")?.Value ?? "";
            _actions[id] = new AnimationAction(actionXml);
        }

        return true;
    }

    /// @see AnimationData.as::getAction
    public AnimationAction? GetAction(IActionDefinition definition)
    {
        _actions.TryGetValue(definition.Id, out AnimationAction? action);
        return action;
    }

    /// @see AnimationData.as::getFrameCount
    public int GetFrameCount(IActionDefinition definition)
    {
        AnimationAction? action = GetAction(definition);
        if (action == null)
        {
            return 0;
        }
        return action.FrameCount;
    }
}
