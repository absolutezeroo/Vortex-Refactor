// @see core/window/services/IMouseListenerService.as

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window.Services;

/// @see core/window/services/IMouseListenerService.as
public interface IMouseListenerService
{
    /// @see core/window/services/IMouseListenerService.as::get eventTypes
    List<string> EventTypes { get; }

    /// @see core/window/services/IMouseListenerService.as::get/set areaLimit
    uint AreaLimit { get; set; }

    /// @see core/window/services/IMouseListenerService.as::dispose
    void DisposeService();

    /// @see core/window/services/IMouseListenerService.as::begin
    IWindow? Begin(IWindow? window, uint flags = 0);

    /// @see core/window/services/IMouseListenerService.as::end
    IWindow? End(IWindow? window);
}
