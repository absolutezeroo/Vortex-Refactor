// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/IAvatarImageListener.as

using IDisposable = Vortex.Core.Runtime.IDisposable;

namespace Vortex.Habbo.Avatar;

/// @see WIN63-202407091256-704579380-Source-main/habbo/avatar/IAvatarImageListener.as
public interface IAvatarImageListener : IDisposable
{
    /// @see IAvatarImageListener.as::avatarImageReady
    void AvatarImageReady(string param1);
}
