// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/ui/IRoomUI.as

using Vortex.Core.Runtime;
using Vortex.Core.Window.Components;
using Vortex.Habbo.Session;

namespace Vortex.Habbo.UI;

/// @see com.sulake.habbo.ui.IRoomUI
public interface IRoomUI : IUnknown
{
    /// @see IRoomUI.as::createDesktop
    IRoomDesktop? CreateDesktop(IRoomSession? session);

    /// @see IRoomUI.as::get chatContainer
    IDisplayObjectWrapper? ChatContainer { get; }

    /// @see IRoomUI.as::disposeDesktop
    void DisposeDesktop(string identifier);

    /// @see IRoomUI.as::getDesktop
    IRoomDesktop? GetDesktop(string identifier);

    /// @see IRoomUI.as::getActiveCanvasId
    int GetActiveCanvasId(int roomId);

    /// @see IRoomUI.as::set visible
    bool Visible { set; }

    /// @see IRoomUI.as::hideWidget
    void HideWidget(string widgetType);

    /// @see IRoomUI.as::showGamePlayerName
    void ShowGamePlayerName(int userId, string userName, uint color, int roomId);

    /// @see IRoomUI.as::mouseEventPositionHasContextMenu
    bool MouseEventPositionHasContextMenu(object? mouseEvent);

    /// @see IRoomUI.as::triggerbottomBarResize
    void TriggerBottomBarResize();
}
