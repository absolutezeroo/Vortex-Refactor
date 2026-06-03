// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AnimationLayerData.as

using System.Xml.Linq;

using Vortex.Habbo.Avatar.Actions;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AnimationLayerData.as
public class AnimationLayerData : IAnimationLayerData
{
    public const string TYPE_BODYPART = "bodypart";
    public const string TYPE_FX = "fx";

    /// @see AnimationLayerData.as::AnimationLayerData
    public AnimationLayerData(XElement xml, string type, IActionDefinition? actionDefinition)
    {
        Items = new Dictionary<string, string>();

        Id = (string?)xml.Attribute("id") ?? "";
        AnimationFrame = (int?)xml.Attribute("frame") ?? 0;
        Dx = (int?)xml.Attribute("dx") ?? 0;
        Dy = (int?)xml.Attribute("dy") ?? 0;
        Dz = (int?)xml.Attribute("dz") ?? 0;
        DirectionOffset = (int?)xml.Attribute("dd") ?? 0;
        Type = type;
        Base = (string?)xml.Attribute("base") ?? "";

        foreach (XElement itemEl in xml.Elements("item"))
        {
            string itemId = (string?)itemEl.Attribute("id") ?? "";
            string itemBase = (string?)itemEl.Attribute("base") ?? "";
            Items[itemId] = itemBase;
        }

        if (actionDefinition == null)
        {
            return;
        }

        ActiveActionData activeAction = new(actionDefinition.State, Base)
        {
            Definition = actionDefinition,
        };
        Action = activeAction;
    }

    /// @see AnimationLayerData.as::get items
    public Dictionary<string, string> Items { get; }

    /// @see AnimationLayerData.as::get id
    public string Id { get; }

    /// @see AnimationLayerData.as::get animationFrame
    public int AnimationFrame { get; }

    /// @see AnimationLayerData.as::get dx
    public int Dx { get; }

    /// @see AnimationLayerData.as::get dy
    public int Dy { get; }

    /// @see AnimationLayerData.as::get dz
    public int Dz { get; }

    /// @see AnimationLayerData.as::get directionOffset
    public int DirectionOffset { get; }

    /// @see AnimationLayerData.as::get type
    public string Type { get; }

    /// @see AnimationLayerData.as::get base
    public string Base { get; }

    /// @see AnimationLayerData.as::get action
    public IActiveActionData? Action { get; }
}
