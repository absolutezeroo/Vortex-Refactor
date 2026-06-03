// @see habbo/window/widgets/PetImageWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/PetImageWidget.as
public class PetImageWidget : IClass3654, IWidget
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;
    private string _figure = "1 0 ffffff";
    private int _scale = 64;
    private int _direction = 2;
    private double _zoomX = 1;
    private double _zoomY = 1;

    /// @see habbo/window/widgets/PetImageWidget.as::PetImageWidget
    public PetImageWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/PetImageWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/PetImageWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/PetImageWidget.as::figure
    public string? Figure
    {
        get => _figure;
        set
        {
            if (value != null)
            {
                _figure = value;
                Refresh();
            }
        }
    }

    /// @see habbo/window/widgets/PetImageWidget.as::scale
    public int Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/PetImageWidget.as::direction
    public int Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/PetImageWidget.as::zoomX
    public double ZoomX
    {
        get => _zoomX;
        set
        {
            _zoomX = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/PetImageWidget.as::zoomY
    public double ZoomY
    {
        get => _zoomY;
        set
        {
            _zoomY = value;
            Refresh();
        }
    }

    /// @see habbo/window/widgets/PetImageWidget.as::petWidth
    public int PetWidth => 0;

    /// @see habbo/window/widgets/PetImageWidget.as::petHeight
    public int PetHeight => 0;

    /// @see habbo/window/widgets/PetImageWidget.as::shrinkOnOverflow
    public bool ShrinkOnOverflow { get; set; }

    /// @see habbo/window/widgets/PetImageWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
    }

    /// @see habbo/window/widgets/PetImageWidget.as::refresh
    private static void Refresh()
    {
        // TODO(window-port): Depends on room engine pet image rendering.
        // AS3 creates pet image from figure string, scales it, and sets on bitmap wrapper.
    }
}
