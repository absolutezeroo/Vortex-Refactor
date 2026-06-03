// @see habbo/window/widgets/RoomUserCountWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// <summary>
/// Nearly empty implementation matching the AS3 skeleton.
/// The AS3 source itself is a minimal stub with no real logic.
/// </summary>
/// @see habbo/window/widgets/RoomUserCountWidget.as
public class RoomUserCountWidget : IRoomUserCountWidget, IWidget
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;

    /// @see habbo/window/widgets/RoomUserCountWidget.as::RoomUserCountWidget
    public RoomUserCountWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/RoomUserCountWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/RoomUserCountWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/RoomUserCountWidget.as::userCount
    int IRoomUserCountWidget.UserCount
    {
        set { } // No-op in AS3 source — the widget is a skeleton.
    }

    /// @see habbo/window/widgets/RoomUserCountWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
    }
}
