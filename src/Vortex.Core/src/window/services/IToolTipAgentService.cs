// @see core/window/services/IToolTipAgentService.as

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window.Services;

/// @see core/window/services/IToolTipAgentService.as
public interface IToolTipAgentService
{
    /// @see core/window/services/IToolTipAgentService.as::dispose
    void DisposeService();

    /// @see core/window/services/IToolTipAgentService.as::begin
    IWindow? Begin(IWindow? window, uint flags = 0);

    /// @see core/window/services/IToolTipAgentService.as::end
    IWindow? End(IWindow? window);

    /// @see core/window/services/IToolTipAgentService.as::updateCaption
    void UpdateCaption(IWindow? window);
}
