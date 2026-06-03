// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3526.as

using Vortex.Habbo.Avatar.Actions;

namespace Vortex.Habbo.Avatar.Animation;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/animation/class_3526.as
public interface IAnimationLayerData
{
    /// @see class_3526.as::get id
    string Id { get; }

    /// @see class_3526.as::get action
    IActiveActionData? Action { get; }

    /// @see class_3526.as::get animationFrame
    int AnimationFrame { get; }

    /// @see class_3526.as::get dx
    int Dx { get; }

    /// @see class_3526.as::get dy
    int Dy { get; }

    /// @see class_3526.as::get dz
    int Dz { get; }

    /// @see class_3526.as::get directionOffset
    int DirectionOffset { get; }
}
