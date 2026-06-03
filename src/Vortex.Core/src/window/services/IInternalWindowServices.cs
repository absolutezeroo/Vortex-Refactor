// @see core/window/services/IInternalWindowServices.as

namespace Vortex.Core.Window.Services;

/// @see core/window/services/IInternalWindowServices.as
public interface IInternalWindowServices
{
    /// @see core/window/services/IInternalWindowServices.as::getMouseDraggingService
    WindowMouseDragger? GetMouseDraggingService();

    /// @see core/window/services/IInternalWindowServices.as::getMouseScalingService
    WindowMouseScaler? GetMouseScalingService();

    /// @see core/window/services/IInternalWindowServices.as::getMouseListenerService
    WindowMouseListener? GetMouseListenerService();

    /// @see core/window/services/IInternalWindowServices.as::getFocusManagerService
    IFocusManagerService? GetFocusManagerService();

    /// @see core/window/services/IInternalWindowServices.as::getToolTipAgentService
    WindowToolTipAgent? GetToolTipAgentService();

    /// @see core/window/services/IInternalWindowServices.as::getGestureAgentService
    GestureAgentService? GetGestureAgentService();
}
