// @see core/window/services/IMouseScalingService.as

namespace Vortex.Core.Window.Services;

/// @see core/window/services/IMouseScalingService.as
public interface IMouseScalingService
{
    /// @see core/window/services/IMouseScalingService.as::dispose
    void DisposeService();

    /// @see core/window/services/IMouseScalingService.as::begin
    IWindow? Begin(IWindow? window, uint flags = 0);

    /// @see core/window/services/IMouseScalingService.as::end
    IWindow? End(IWindow? window);
}
