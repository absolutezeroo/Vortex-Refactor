// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AnimationManager.as

using System.Xml.Linq;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/AnimationManager.as
public class AnimationManager : IAnimationManager
{
    private readonly Dictionary<string, Animation> _animations;

    /// @see AnimationManager.as::AnimationManager
    public AnimationManager()
    {
        _animations = new Dictionary<string, Animation>();
    }

    /// @see AnimationManager.as::registerAnimation
    public bool RegisterAnimation(AvatarStructure structure, XElement xml)
    {
        string name = (string?)xml.Attribute("name") ?? "";
        _animations[name] = new Animation(structure, xml);

        return true;
    }

    /// @see AnimationManager.as::getAnimation
    public Animation? GetAnimation(string name)
    {
        _animations.TryGetValue(name, out Animation? animation);

        return animation;
    }

    /// @see AnimationManager.as::getLayerData
    public AnimationLayerData? GetLayerData(string animation, int frame, string part)
    {
        if (!_animations.TryGetValue(animation, out Animation? anim))
        {
            return null;
        }

        return anim.GetLayerData(frame, part);
    }

    /// @see AnimationManager.as::get animations
    public IDictionary<string, Animation> Animations => _animations;
}
