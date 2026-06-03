// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as
public class LimitedItemSupplyLeftOverlayWidget : ILimitedItemSupplyLeftOverlayWidget, IWidget, ILimitedItemOverlayWidget
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::LimitedItemSupplyLeftOverlayWidget
    public LimitedItemSupplyLeftOverlayWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::supplyLeft
    public int SupplyLeft { get; set; }

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::serialNumber
    public int SerialNumber
    {
        get => 0;
        set { } // Supply-left overlay uses supplyLeft, not serialNumber.
    }

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::seriesSize
    public int SeriesSize { get; set; }

    /// @see habbo/window/widgets/LimitedItemSupplyLeftOverlayWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
    }
}
