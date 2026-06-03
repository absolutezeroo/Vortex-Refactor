// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as
public class LimitedItemPreviewOverlayWidget : ILimitedItemPreviewOverlayWidget, IWidget, ILimitedItemOverlayWidget
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;

    /// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as::LimitedItemPreviewOverlayWidget
    public LimitedItemPreviewOverlayWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as::serialNumber
    public int SerialNumber
    {
        get;
        set;
        // TODO(window-port): Update bitmap overlay via class_3723/LimitedItemOverlayNumberBitmapGenerator.
    }

    /// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as::seriesSize
    public int SeriesSize
    {
        get;
        set;
        // TODO(window-port): Update bitmap overlay via class_3723/LimitedItemOverlayNumberBitmapGenerator.
    }

    /// @see habbo/window/widgets/LimitedItemPreviewOverlayWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
    }
}
