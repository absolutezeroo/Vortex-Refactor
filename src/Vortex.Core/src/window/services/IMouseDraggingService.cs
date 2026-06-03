// @see core/window/services/IMouseDraggingService.as

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window.Services;

/// @see core/window/services/IMouseDraggingService.as
public interface IMouseDraggingService
{
    /// @see core/window/services/IMouseDraggingService.as::dispose
    void Dispose();

    /// @see core/window/services/IMouseDraggingService.as::begin
    IWindow? Begin(IWindow window, uint flags = 0);

    /// @see core/window/services/IMouseDraggingService.as::end
    IWindow? End(IWindow window);
}
