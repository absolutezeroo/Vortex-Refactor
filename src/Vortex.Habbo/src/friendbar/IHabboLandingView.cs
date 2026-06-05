// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/IHabboLandingView.as

using Vortex.Core.Runtime;

namespace Vortex.Habbo.FriendBar;

/// @see com.sulake.habbo.friendbar.IHabboLandingView
public interface IHabboLandingView : IUnknown
{
    /// @see IHabboLandingView.as::activate
    void Activate();

    /// @see IHabboLandingView.as::disable
    void Disable();
}
