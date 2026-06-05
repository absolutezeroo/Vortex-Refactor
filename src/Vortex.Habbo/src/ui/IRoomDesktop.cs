// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/habbo/ui/IRoomDesktop.as

using Vortex.Core.Runtime.Events;
using Vortex.Habbo.Session;

namespace Vortex.Habbo.UI;

/// @see com.sulake.habbo.ui.IRoomDesktop
public interface IRoomDesktop
{
    /// @see IRoomDesktop.as::get events
    EventDispatcherWrapper events { get; }

    /// @see IRoomDesktop.as::get roomSession
    IRoomSession roomSession { get; }

    /// @see RoomDesktop.as::get layoutManager
    DesktopLayoutManager layoutManager { get; }

    /// @see RoomDesktop.as::set visible
    bool visible { set; }

    /// @see RoomDesktop.as::createRoomView
    void CreateRoomView(int canvasId);

    /// @see RoomDesktop.as::processEvent
    void ProcessEvent(object? eventObj);

    /// @see RoomDesktop.as::createWidget
    void CreateWidget(string widgetType);

    /// @see RoomDesktop.as::disposeWidget
    void DisposeWidget(string widgetType);

    /// @see RoomDesktop.as::getWidgetState
    int GetWidgetState(string widgetType);

    /// @see RoomDesktop.as::update
    void Update();
}
