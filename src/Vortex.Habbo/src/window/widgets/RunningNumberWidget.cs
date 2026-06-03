// @see habbo/window/widgets/RunningNumberWidget.as

using Vortex.Core.Runtime;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/RunningNumberWidget.as
public class RunningNumberWidget : IRunningNumberWidget, IWidget, IUpdateReceiver
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;
    private double _displayedNumber;

    /// @see habbo/window/widgets/RunningNumberWidget.as::RunningNumberWidget
    public RunningNumberWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/RunningNumberWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/RunningNumberWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/RunningNumberWidget.as::number
    public int Number { get; set; }

    /// @see habbo/window/widgets/RunningNumberWidget.as::digits
    public uint Digits { get; set; } = 8;

    /// @see habbo/window/widgets/RunningNumberWidget.as::colorStyle
    public int ColorStyle { get; set; }

    /// @see habbo/window/widgets/RunningNumberWidget.as::updateFrequency
    public int UpdateFrequency { get; set; } = 50;

    /// @see habbo/window/widgets/RunningNumberWidget.as::initialNumber
    public int InitialNumber
    {
        set
        {
            Number = value;
            _displayedNumber = value;
        }
    }

    /// <summary>
    /// Incrementally moves _displayedNumber toward _number each frame.
    /// </summary>
    /// @see habbo/window/widgets/RunningNumberWidget.as::update
    public void Update(uint param1)
    {
        if (disposed)
        {
            return;
        }

        if (System.Math.Abs(_displayedNumber - Number) < 0.5)
        {
            _displayedNumber = Number;
            return;
        }

        double diff = Number - _displayedNumber;
        double step = diff / UpdateFrequency;
        if (System.Math.Abs(step) < 1.0)
        {
            step = diff > 0 ? 1.0 : -1.0;
        }
        _displayedNumber += step;

        // TODO(window-port): Update visual digit display
    }

    /// @see habbo/window/widgets/RunningNumberWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
    }
}
