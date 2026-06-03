// @see core/window/graphics/class_3354.as

using Godot;

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Window renderer interface.
/// </summary>
/// @see core/window/graphics/class_3354.as
public interface IClass3354
{
    bool Disposed { get; }

    bool Debug { get; set; }

    ISkinContainer SkinContainer { get; }

    void Render();

    void AddToRenderQueue(IWindow window, Rect2? dirtyRegion, uint flags);

    void FlushRenderQueue();

    void Invalidate(IWindowContext context, Rect2? region);

    Image? GetDrawBufferForRenderable(IWindow window);

    void Purge(IWindow? window = null, bool keepVisible = true);

    void Dispose();
}
