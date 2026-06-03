// @see core/window/services/ServiceManager.as

using System;

namespace Vortex.Core.Window.Services;

/// <summary>
/// Factory and accessor for all window interaction services.
/// Creates and owns the 6 service instances (dragger, scaler, listener,
/// focus manager, tooltip agent, gesture agent).
/// </summary>
/// @see core/window/services/ServiceManager.as
public class ServiceManager : IInternalWindowServices, IDisposable
{
    /// @see ServiceManager.as — stored context reference for services that need it
    private IWindowContext? _windowContext;

    /// @see ServiceManager.as::ServiceManager
    public ServiceManager(IWindowContext? windowContext = null)
    {
        _windowContext = windowContext;

        Dragger = new WindowMouseDragger();
        Scaler = new WindowMouseScaler();
        Listener = new WindowMouseListener();
        FocusManagerService = new FocusManager();
        ToolTipAgent = new WindowToolTipAgent();
        GestureAgent = new GestureAgentService();

        if (windowContext != null)
        {
            ToolTipAgent.SetWindowContext(windowContext);
        }
    }

    /// @see ServiceManager.as::get disposed
    public bool Disposed { get; private set; }

    /// @see ServiceManager.as::dispose
    public void DisposeService()
    {
        if (Disposed)
        {
            return;
        }

        Dragger?.Dispose();
        Dragger = null;

        Scaler?.DisposeService();
        Scaler = null;

        Listener?.DisposeService();
        Listener = null;

        ((IDisposable?)FocusManagerService)?.Dispose();
        FocusManagerService = null;

        ToolTipAgent?.DisposeService();
        ToolTipAgent = null;

        GestureAgent?.DisposeService();
        GestureAgent = null;

        _windowContext = null;
        Disposed = true;
    }

    void IDisposable.Dispose()
    {
        DisposeService();
        GC.SuppressFinalize(this);
    }

    /// @see ServiceManager.as::getMouseDraggingService
    public WindowMouseDragger? GetMouseDraggingService()
    {
        return Dragger;
    }

    /// @see ServiceManager.as::getMouseScalingService
    public WindowMouseScaler? GetMouseScalingService()
    {
        return Scaler;
    }

    /// @see ServiceManager.as::getMouseListenerService
    public WindowMouseListener? GetMouseListenerService()
    {
        return Listener;
    }

    /// @see ServiceManager.as::getFocusManagerService
    public IFocusManagerService? GetFocusManagerService()
    {
        return FocusManagerService;
    }

    /// @see ServiceManager.as::getToolTipAgentService
    public WindowToolTipAgent? GetToolTipAgentService()
    {
        return ToolTipAgent;
    }

    /// @see ServiceManager.as::getGestureAgentService
    public GestureAgentService? GetGestureAgentService()
    {
        return GestureAgent;
    }

    public WindowMouseDragger? Dragger { get; private set; }

    public WindowMouseScaler? Scaler { get; private set; }

    public WindowMouseListener? Listener { get; private set; }

    public FocusManager? FocusManagerService { get; private set; }

    public WindowToolTipAgent? ToolTipAgent { get; private set; }

    public GestureAgentService? GestureAgent { get; private set; }
}
