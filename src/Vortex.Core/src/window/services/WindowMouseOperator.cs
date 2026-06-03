// @see core/window/services/WindowMouseOperator.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Services;

using WinController = WindowController;

/// <summary>
/// Base class for mouse interaction services (scaling, listening, tooltips).
/// Godot adaptation: uses Input polling in Handler() instead of Flash stage event listeners.
/// The existing WindowMouseDragger is standalone; this base serves scaler/listener/tooltip.
/// </summary>
/// @see core/window/services/WindowMouseOperator.as
public class WindowMouseOperator : IDisposable
{
    protected WinController? _window;
    protected bool _active;
    protected Vector2 _offset;
    protected Vector2 _mouse;
    protected Vector2 _workingPoint;
    protected uint _flags;

    /// @see WindowMouseOperator.as::WindowMouseOperator
    public WindowMouseOperator()
    {
        _workingPoint = Vector2.Zero;
        _mouse = Vector2.Zero;
        _offset = Vector2.Zero;
        _active = false;
        _flags = 0;
    }

    /// @see WindowMouseOperator.as::get disposed
    public bool Disposed { get; private set; }

    /// @see WindowMouseOperator.as::dispose
    public virtual void DisposeService()
    {
        End(_window);
        Disposed = true;
    }

    void IDisposable.Dispose()
    {
        DisposeService();
        GC.SuppressFinalize(this);
    }

    /// @see WindowMouseOperator.as::begin
    public virtual IWindow? Begin(IWindow? window, uint flags = 0)
    {
        _flags = flags;
        WinController? previous = _window;

        if (_window != null)
        {
            End(_window);
        }

        if (window is { disposed: false })
        {
            Vector2 mousePos = GetCurrentMousePosition();
            _mouse.X = mousePos.X;
            _mouse.Y = mousePos.Y;
            _window = (WinController)window;
            GetMousePositionRelativeTo(window, _mouse, ref _offset);
            _active = true;

            // @see WindowMouseOperator.as::begin — register WE_DESTROYED listener
            _window.AddEventListener(WindowEvent.WE_DESTROYED, OnClientWindowDestroyed);
        }

        return previous;
    }

    /// @see WindowMouseOperator.as::end
    public virtual IWindow? End(IWindow? window)
    {
        WinController? previous = _window;

        if (_active)
        {
            if (_window == window)
            {
                // @see WindowMouseOperator.as::end — only RemoveEventListener if not disposed
                if (!_window.disposed)
                {
                    _window.RemoveEventListener(WindowEvent.WE_DESTROYED, OnClientWindowDestroyed);
                }

                _window = null;
                _active = false;
            }
        }

        return previous;
    }

    /// <summary>
    /// Called each frame to check for mouse movement and dispatch operate().
    /// Godot adaptation: called from WindowContext._Process() instead of Flash enterFrame.
    /// </summary>
    /// @see WindowMouseOperator.as::handler
    public virtual void Handler()
    {
        if (!_active)
        {
            return;
        }

        if (_window == null || _window.disposed)
        {
            End(_window);
            return;
        }

        Vector2 mousePos = GetCurrentMousePosition();

        if (_mouse.X != mousePos.X || _mouse.Y != mousePos.Y)
        {
            Operate((int)mousePos.X, (int)mousePos.Y);
            _mouse.X = mousePos.X;
            _mouse.Y = mousePos.Y;
        }
    }

    /// <summary>
    /// Called when mouse button is released. Ends the operation.
    /// Godot adaptation: called from MouseEventProcessor on mouseUp instead of Flash mouseUp listener.
    /// </summary>
    public virtual void HandleMouseUp()
    {
        if (_active)
        {
            End(_window);
        }
    }

    /// @see WindowMouseOperator.as::operate
    public virtual void Operate(int mouseX, int mouseY)
    {
        _mouse.X = mouseX;
        _mouse.Y = mouseY;
        GetMousePositionRelativeTo(_window!, _mouse, ref _workingPoint);
        _window!.Offset(_workingPoint.X - _offset.X, _workingPoint.Y - _offset.Y);
    }

    /// @see WindowMouseOperator.as::clientWindowDestroyed
    virtual internal void ClientWindowDestroyed()
    {
        End(_window);
    }

    private void OnClientWindowDestroyed(WindowEvent param1, IWindow param2)
    {
        ClientWindowDestroyed();
    }

    /// @see WindowMouseOperator.as::getMousePositionRelativeTo
    protected static void GetMousePositionRelativeTo(IWindow window, Vector2 mouse, ref Vector2 result)
    {
        if (window is WindowController wc)
        {
            Vector2 globalPos = wc.GetGlobalPosition();
            result.X = mouse.X - globalPos.X;
            result.Y = mouse.Y - globalPos.Y;
        }
        else
        {
            result.X = mouse.X;
            result.Y = mouse.Y;
        }
    }

    /// <summary>
    /// Gets the current mouse screen position from Godot input system.
    /// </summary>
    protected static Vector2 GetCurrentMousePosition()
    {
        return DisplayServer.MouseGetPosition();
    }
}
