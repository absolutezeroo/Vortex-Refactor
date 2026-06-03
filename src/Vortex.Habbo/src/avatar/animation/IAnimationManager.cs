// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3372.as

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3372.as
public interface IAnimationManager
{
    /// @see class_3372.as::get animations
    IDictionary<string, Animation> Animations { get; }

    /// @see class_3372.as::getAnimation
    Animation? GetAnimation(string name);

    /// @see class_3372.as::getLayerData
    AnimationLayerData? GetLayerData(string animation, int frame, string part);
}
