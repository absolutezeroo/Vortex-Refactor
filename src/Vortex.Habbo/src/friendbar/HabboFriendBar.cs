// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/friendbar/HabboFriendBar.as

using Vortex.Core.Runtime;
using Vortex.Habbo.FriendBar.LandingView;
using Vortex.IID;

namespace Vortex.Habbo.FriendBar;

/// @see com.sulake.habbo.friendbar.HabboFriendBar
public class HabboFriendBar : Component, IHabboFriendBar
{
    /// @see HabboFriendBar.as::HabboFriendBar
    public HabboFriendBar(IContext param1, uint param2 = 0, object? param3 = null) : base(param1, param2, param3)
    {
        // TODO(as3-port): HabboFriendBarData sub-component not yet ported.
        // TODO(as3-port): HabboFriendBarView sub-component not yet ported.
        param1.AttachComponent(new HabboLandingView(param1, param2, param3), [new IIDHabboLandingView()]);
        // TODO(as3-port): HabboTalent sub-component not yet ported.
        // TODO(as3-port): HabboEpicPopupView sub-component not yet ported.
        // TODO(as3-port): GroupForumController sub-component not yet ported.
    }

    // TODO(as3-port): visible setter delegates to IHabboFriendBarView, not yet ported.
}
