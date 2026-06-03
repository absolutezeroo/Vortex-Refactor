// @see core/window/graphics/IGraphicContext.as

using System;

using Godot;

namespace Vortex.Core.Window.Graphics;

/// @see core/window/graphics/IGraphicContext.as
public interface IGraphicContext : IDisposable
{
    new void Dispose();

    Node2D? DisplayNode { get; }

    bool Visible { get; set; }

    float Blend { get; set; }

    /// Godot adaptation: color modulate applied to the display node (maps to Flash ColorTransform on display object)
    Color Modulate { get; set; }

    bool Mouse { get; set; }

    object?[]? Filters { get; set; }

    int NumChildContexts { get; }

    void Offset(Vector2 point);

    Rect2 GetDrawRegion();

    Image? SetDrawRegion(Rect2 region, bool reallocate, Rect2? clipRegion);

    Image? FetchDrawBuffer();

    void ShowRedrawRegion(Rect2? region);

    void UpdateDisplayTexture();

    IGraphicContext AddChildContext(IGraphicContext child);

    IGraphicContext AddChildContextAt(IGraphicContext child, int index);

    IGraphicContext GetChildContextAt(int index);

    int GetChildContextIndex(IGraphicContext child);

    IGraphicContext RemoveChildContext(IGraphicContext child);

    IGraphicContext? RemoveChildContextAt(int index);

    void SetChildContextIndex(IGraphicContext child, int index);

    void SwapChildContexts(IGraphicContext child1, IGraphicContext child2);

    void SwapChildContextsAt(int index1, int index2);
}
