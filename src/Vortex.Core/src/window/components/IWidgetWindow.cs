// @see core/window/components/IWidgetWindow.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IWidgetWindow.as
public interface IWidgetWindow : IWindow, IIterable
{
    /// @see core/window/components/IWidgetWindow.as::get widget
    IWidget? Widget();

    /// @see core/window/components/IWidgetWindow.as::get rootWindow
    IWindow? RootWindow();

    /// @see core/window/components/IWidgetWindow.as::set rootWindow
    void RootWindow(object? value);
}
