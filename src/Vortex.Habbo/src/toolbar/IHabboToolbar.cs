using Godot;

using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;

namespace Vortex.Habbo.Toolbar;

/// @see com.sulake.habbo.toolbar.IHabboToolbar
public interface IHabboToolbar : IUnknown
{
    /// @see IHabboToolbar.as::get events
    EventDispatcherWrapper? events { get; }

    /// @see IHabboToolbar.as::get toolBarAreaWidth
    int toolBarAreaWidth { get; }

    /// @see IHabboToolbar.as::get onDuty
    bool onDuty { get; }

    /// @see IHabboToolbar.as::setToolbarState
    void SetToolbarState(int state);

    /// @see IHabboToolbar.as::toggleWindowVisibility
    bool ToggleWindowVisibility(string windowId);

    /// @see IHabboToolbar.as::setIconBitmap
    void SetIconBitmap(int iconId, Image? bitmap);

    /// @see IHabboToolbar.as::getRect
    Rect2I GetRect();

    /// @see IHabboToolbar.as::setIconVisibility
    void SetIconVisibility(int iconId, bool visible);

    /// @see IHabboToolbar.as::createTransitionToIcon
    void CreateTransitionToIcon(int iconId);

    /// @see IHabboToolbar.as::getIconLocation
    Vector2I GetIconLocation(int iconId);
}
