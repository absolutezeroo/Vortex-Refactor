// @see WIN63-202407091256-704579380-Source-main/core/window/services/WindowMouseDragger.as
// @see WIN63-202407091256-704579380-Source-main/core/window/services/IMouseDraggingService.as

using System;

using Godot;

namespace Vortex.Core.Window.Services;

/// @see WIN63-202407091256-704579380-Source-main/core/window/services/WindowMouseDragger.as
public class WindowMouseDragger : IDisposable
{
    private Vector2 _offset;

    /// @see IMouseDraggingService.as::begin
    public IWindow? Begin(IWindow window, uint flags = 0)
    {
        _ = flags;

        if (IsDragging && DragTarget != null)
        {
            End(DragTarget);
        }

        DragTarget = window;
        IsDragging = true;
        return window;
    }

    /// @see IMouseDraggingService.as::end
    public IWindow? End(IWindow window)
    {
        if (DragTarget != window)
        {
            return null;
        }

        IWindow? previous = DragTarget;
        DragTarget = null;
        IsDragging = false;
        _offset = Vector2.Zero;
        return previous;
    }

    /// @see WindowMouseDragger.as::operate
    /// AS3: getMousePositionRelativeTo(_window, _mouse, var_1893);
    ///      _window.offset(var_1893.x - _offset.x, var_1893.y - _offset.y);
    public void Operate(int mouseX, int mouseY)
    {
        if (!IsDragging || DragTarget == null || DragTarget.disposed)
        {
            return;
        }

        // Compute mouse position relative to window's own global origin (matching AS3 getMousePositionRelativeTo)
        Vector2 globalPos = GetGlobalPosition(DragTarget);
        float relativeX = mouseX - globalPos.X;
        float relativeY = mouseY - globalPos.Y;

        DragTarget.Offset(relativeX - _offset.X, relativeY - _offset.Y);
    }

    /// @see WindowMouseDragger.as — set the initial mouse offset within the window
    public void SetOffset(float offsetX, float offsetY)
    {
        _offset = new Vector2(offsetX, offsetY);
    }

    /// @see IMouseDraggingService.as::dispose
    public void Dispose()
    {
        DragTarget = null;
        IsDragging = false;
        _offset = Vector2.Zero;
    }

    public bool IsDragging { get; private set; }

    public IWindow? DragTarget { get; private set; }

    private static Vector2 GetGlobalPosition(IWindow window)
    {
        float gx = window.x;
        float gy = window.y;
        IWindow? current = window.parent;
        while (current != null)
        {
            gx += current.x;
            gy += current.y;
            current = current.parent;
        }
        return new Vector2(gx, gy);
    }
}
