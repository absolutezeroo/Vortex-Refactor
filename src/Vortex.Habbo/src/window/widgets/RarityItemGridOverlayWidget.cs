// @see habbo/window/widgets/RarityItemGridOverlayWidget.as

using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Iterators;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/RarityItemGridOverlayWidget.as
public class RarityItemGridOverlayWidget : IRarityItemGridOverlayWidget, IWidget, IRarityItemOverlayWidget
{
    private readonly IWidgetWindow _widgetWindow;
    private readonly HabboWindowManagerComponent _windowManager;

    /// @see habbo/window/widgets/RarityItemGridOverlayWidget.as::RarityItemGridOverlayWidget
    public RarityItemGridOverlayWidget(IWidgetWindow widgetWindow, HabboWindowManagerComponent windowManager)
    {
        _widgetWindow = widgetWindow;
        _windowManager = windowManager;
    }

    /// @see habbo/window/widgets/RarityItemGridOverlayWidget.as::get disposed
    public bool disposed { get; private set; }

    /// @see habbo/window/widgets/RarityItemGridOverlayWidget.as::get iterator
    public object? Iterator()
    {
        return EmptyIterator.INSTANCE;
    }

    /// @see habbo/window/widgets/RarityItemGridOverlayWidget.as::rarityLevel
    public int RarityLevel
    {
        get;
        set;
        // TODO(window-port): Update rarity overlay bitmap.
    }

    /// @see habbo/window/widgets/RarityItemGridOverlayWidget.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
    }
}
