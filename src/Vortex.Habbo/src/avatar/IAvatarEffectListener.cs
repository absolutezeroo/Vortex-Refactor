// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3375.as

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/class_3375.as
public interface IAvatarEffectListener : IDisposable
{
    /// @see class_3375.as::avatarEffectReady
    void AvatarEffectReady(int param1);
}
