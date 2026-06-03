// @see core/window/services/FocusManager.as

using System;

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window.Services;

/// <summary>
/// Manages focusable windows. Tracks a list of IFocusWindow instances
/// and resolves focus when the current focus target is lost.
/// Godot adaptation: no stage focus events; focus resolution is called
/// explicitly from MouseEventProcessor on click-away.
/// </summary>
/// @see core/window/services/FocusManager.as
public class FocusManager : IFocusManagerService
{
    private List<IFocusWindow>? _focusWindows = new();

    /// @see FocusManager.as::FocusManager
    public FocusManager() { }

    /// @see FocusManager.as::get disposed
    public bool Disposed { get; private set; }

    /// Godot adaptation: current focused window (replaces AS3 var_1859.focus)
    public IFocusWindow? CurrentFocus { get; private set; }

    /// @see FocusManager.as::dispose
    public void DisposeService()
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;
        CurrentFocus = null;
        _focusWindows = null;
    }

    void IDisposable.Dispose()
    {
        DisposeService();

        GC.SuppressFinalize(this);
    }

    /// @see FocusManager.as::registerFocusWindow — auto-focus only if no current focus
    public void RegisterFocusWindow(IFocusWindow? window)
    {
        if (window == null || _focusWindows == null)
        {
            return;
        }

        if (!_focusWindows.Contains(window))
        {
            _focusWindows.Add(window);
        }

        // @see FocusManager.as — AS3 checks var_1859.focus == null (stage focus)
        if (CurrentFocus != null)
        {
            return;
        }

        window.Focus();

        CurrentFocus = window;
    }

    /// @see FocusManager.as::removeFocusWindow
    public void RemoveFocusWindow(IFocusWindow? window)
    {
        if (window == null || _focusWindows == null)
        {
            return;
        }

        int index = _focusWindows.IndexOf(window);

        if (index > -1)
        {
            _focusWindows.RemoveAt(index);
        }

        // @see FocusManager.as — AS3 checks if focus is null after removal, only then resolves
        if (window == CurrentFocus)
        {
            CurrentFocus = null;
        }

        if (CurrentFocus == null)
        {
            ResolveNextFocusTarget();
        }
    }

    /// @see FocusManager.as::resolveNextFocusTarget
    public IFocusWindow? ResolveNextFocusTarget()
    {
        if (_focusWindows == null)
        {
            return null;
        }

        IFocusWindow? target = null;
        int i = _focusWindows.Count;

        while (i-- > 0)
        {
            target = _focusWindows[i];

            if (target is IWindow { disposed: false })
            {
                target.Focus();
                CurrentFocus = target;

                break;
            }

            // Disposed window — remove from list
            _focusWindows.RemoveAt(i);

            target = null;
        }

        return target;
    }
}
