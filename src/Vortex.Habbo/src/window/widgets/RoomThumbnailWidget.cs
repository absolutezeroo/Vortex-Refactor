// @see habbo/window/widgets/RoomThumbnailWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// <summary>
/// Nearly empty implementation matching the AS3 skeleton.
/// The AS3 source itself is a minimal stub with no real logic.
/// </summary>
/// @see habbo/window/widgets/RoomThumbnailWidget.as
public class RoomThumbnailWidget : IRoomThumbnailWidget, IWidget
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;

    /// @see habbo/window/widgets/RoomThumbnailWidget.as::RoomThumbnailWidget
    public RoomThumbnailWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/RoomThumbnailWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/RoomThumbnailWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/RoomThumbnailWidget.as::flatId
    int IRoomThumbnailWidget.FlatId
    {
        set { } // No-op in AS3 source — the widget is a skeleton.
    }

    /// @see habbo/window/widgets/RoomThumbnailWidget.as::reset
    void IRoomThumbnailWidget.Reset()
    {
        // No-op in AS3 source — the widget is a skeleton.
    }

    /// @see habbo/window/widgets/RoomThumbnailWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
    }
}
