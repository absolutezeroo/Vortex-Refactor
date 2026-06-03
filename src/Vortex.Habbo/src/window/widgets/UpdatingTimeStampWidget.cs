// @see habbo/window/widgets/UpdatingTimeStampWidget.as

using Vortex.Core.Runtime;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/UpdatingTimeStampWidget.as
public class UpdatingTimeStampWidget : IClass3614, IWidget, IUpdateReceiver
{
    private const uint UPDATE_INTERVAL_MS = 60000;

    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;
    private double _timeStamp;
    private uint _elapsedSinceUpdate;

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::UpdatingTimeStampWidget
    public UpdatingTimeStampWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::timeStamp
    public double TimeStamp
    {
        get => _timeStamp;
        set
        {
            _timeStamp = value;
            _elapsedSinceUpdate = 0;
            OnTimerTick();
        }
    }

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::align
    public string? Align
    {
        set
        {
            // TODO(window-port): Apply text alignment to display label
        }
    }

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::reset
    public void Reset()
    {
        _timeStamp = 0;
        _elapsedSinceUpdate = 0;
        OnTimerTick();
    }

    /// <summary>
    /// Called periodically to update the "time ago" display text.
    /// Accumulates elapsed time and refreshes every 60 seconds.
    /// </summary>
    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::update
    public void Update(uint param1)
    {
        if (disposed)
        {
            return;
        }

        _elapsedSinceUpdate += param1;
        if (_elapsedSinceUpdate >= UPDATE_INTERVAL_MS)
        {
            _elapsedSinceUpdate = 0;
            OnTimerTick();
        }
    }

    /// <summary>
    /// Computes the "time ago" text from the stored timestamp and updates the display.
    /// </summary>
    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::onTimerTick
    private static void OnTimerTick()
    {
        // TODO(window-port): Implement "time ago" display — requires localization system
        // Compute elapsed = now - _timeStamp, format as "X minutes/hours/days ago"
    }

    /// @see habbo/window/widgets/UpdatingTimeStampWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
    }
}
