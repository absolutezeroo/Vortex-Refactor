// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as

using Vortex.Core.Runtime;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as
public class LimitedItemGridOverlayWidget : ILimitedItemGridOverlayWidget, IWidget, ILimitedItemOverlayWidget, IUpdateReceiver
{
    private const int SHINE_INTERVAL_MS = 10000;
    private const int SHINE_LENGTH_MS = 250;

    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;
    private uint _lastShineTime;

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::LimitedItemGridOverlayWidget
    public LimitedItemGridOverlayWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::serialNumber
    public int SerialNumber { get; set; }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::seriesSize
    public int SeriesSize
    {
        get => 0;
        set { } // No-op: grid overlay does not use series size.
    }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::animated
    public bool Animated { get; set; }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::update
    public void Update(uint param1)
    {
        if (!Animated || disposed)
        {
            return;
        }

        uint elapsed = param1 - _lastShineTime;
        if (elapsed >= SHINE_INTERVAL_MS)
        {
            _lastShineTime = param1;
            // TODO(window-port): Trigger shine animation effect for SHINE_LENGTH_MS duration.
        }
    }

    /// @see habbo/window/widgets/LimitedItemGridOverlayWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
    }
}
