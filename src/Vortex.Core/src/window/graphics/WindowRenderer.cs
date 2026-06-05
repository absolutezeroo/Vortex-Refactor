// @see core/window/graphics/WindowRenderer.as

using System;
using System.Linq;

using Godot;

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Main render orchestrator. Manages render queue with dirty regions and
/// recursive window tree rendering.
/// </summary>
/// @see core/window/graphics/WindowRenderer.as
public class WindowRenderer : IClass3354, IDisposable
{
    protected const int MAX_DIRTY_REGIONS_PER_WINDOW = 3;
    protected const int MAX_DISTANCE_BEFORE_COMBINE = 10;

    private Dictionary<IWindow, WindowRendererItem> _renderItems;
    private List<IWindow> _renderQueue;
    private List<List<Rect2>> _renderRegions;
    private Rect2 _tempRect;
    private Rect2 _tempRegion;

    /// @see WindowRenderer.as::WindowRenderer
    public WindowRenderer(ISkinContainer skinContainer)
    {
        SkinContainer = skinContainer;
        _renderItems = new Dictionary<IWindow, WindowRendererItem>();
        _renderQueue = new List<IWindow>();
        _renderRegions = new List<List<Rect2>>();
    }

    public bool Disposed { get; private set; }

    /// Diagnostic: number of items currently in the render queue.
    public int RenderQueueCount => _renderQueue.Count;

    /// @see WindowRenderer.as — exposes skin container for default attribute lookup.
    public ISkinContainer SkinContainer { get; }

    public bool Debug { get; set; }

    /// @see WindowRenderer.as::dispose
    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;

        foreach (WindowRendererItem item in _renderItems.Values)
        {
            item.Dispose();
        }

        _renderItems = null!;
        _renderQueue = null!;
        _renderRegions = null!;
    }

    /// @see WindowRenderer.as::purge
    public void Purge(IWindow? window = null, bool keepVisible = true)
    {
        if (window != null)
        {
            if (!window.visible || !keepVisible)
            {
                if (_renderItems.TryGetValue(window, out WindowRendererItem? item))
                {
                    item.Dispose();
                    _renderItems.Remove(window);
                }
                keepVisible = false;
            }

            if (window is not IChildWindowHost container)
            {
                return;
            }

            object? children = container.Children;

            if (children is not IList<IWindow> childList)
            {
                return;
            }

            foreach (IWindow child in childList)
            {
                Purge(child, keepVisible);
            }
        }
        else
        {
            List<IWindow> toRemove = _renderItems.Select(kvp => kvp.Key)
                                                 .Where(w => !w.visible || !keepVisible || (w.parent == null && w is not IDesktopWindow))
                                                 .ToList();

            foreach (IWindow w in toRemove)
            {
                Purge(w, keepVisible);
            }
        }
    }

    /// @see WindowRenderer.as::addToRenderQueue
    public void AddToRenderQueue(IWindow window, Rect2? dirtyRegion, uint flags)
    {
        Rect2 region;

        if (dirtyRegion == null)
        {
            // @see WindowRenderer.as line 286-290 — uses renderingWidth/renderingHeight
            region = new Rect2(0, 0, window.renderingWidth, window.renderingHeight);
        }
        else
        {
            region = dirtyRegion.Value;
        }

        if (region.Size.X <= 0 || region.Size.Y <= 0)
        {
            return;
        }

        if (!GetWindowRendererItem(window).Invalidate(window, flags))
        {
            return;
        }

        // Walk up to find the top-level render target
        if (window.TestParamFlag(16) || window.TestParamFlag(0x40000000))
        {
            IWindow? parent;

            do
            {
                parent = window.parent;

                if (parent == null)
                {
                    return;
                }

                // Check if we reached the desktop window
                if (parent is IDesktopWindow)
                {
                    break;
                }

                if (!parent.visible)
                {
                    return;
                }

                // @see WindowRenderer.as line 324 — offset by renderingX/renderingY
                region = new Rect2(
                    region.Position.X + window.renderingX,
                    region.Position.Y + window.renderingY,
                    region.Size.X, region.Size.Y
                );

                // @see WindowRenderer.as line 322-323 — uses parent renderingWidth/renderingHeight
                int pw = (int)parent.renderingWidth;
                int ph = (int)parent.renderingHeight;

                if (parent.clipping)
                {
                    if (region.Position.X > pw || region.Position.Y > ph ||
                        region.End.X < 0 || region.End.Y < 0)
                    {
                        return;
                    }

                    float x = region.Position.X;
                    float y = region.Position.Y;
                    float w = region.Size.X;
                    float h = region.Size.Y;

                    if (x < 0)
                    {
                        w += x;
                        x = 0;
                    }

                    if (y < 0)
                    {
                        h += y;
                        y = 0;
                    }

                    if (x + w > pw)
                    {
                        w = pw - x;
                    }

                    if (y + h > ph)
                    {
                        h = ph - y;
                    }

                    region = new Rect2(x, y, w, h);
                }

                if (region.Size.X <= 0 || region.Size.Y <= 0)
                {
                    return;
                }

                window = parent;
            }
            while (window.TestParamFlag(16) || window.TestParamFlag(0x40000000));
        }

        // Mark top-level window for cascade
        GetWindowRendererItem(window).Invalidate(window, Class3655.CASCADE);

        // Merge into render queue
        int queueIndex = _renderQueue.IndexOf(window);

        if (queueIndex > -1)
        {
            List<Rect2> regions = _renderRegions[queueIndex];
            Rect2 newRegion = region;

            // Cap at max dirty regions — merge if over limit
            if (regions.Count > MAX_DIRTY_REGIONS_PER_WINDOW)
            {
                newRegion = newRegion.Merge(regions[^1]);
                regions.RemoveAt(regions.Count - 1);
            }

            // Try to merge with nearby regions
            for (int i = 0;
                 i < regions.Count;)
            {
                if (AreRectanglesCloseEnough(regions[i], newRegion, MAX_DISTANCE_BEFORE_COMBINE))
                {
                    newRegion = newRegion.Merge(regions[i]);
                    regions.RemoveAt(i);
                    i = 0; // restart merge check
                }
                else
                {
                    i++;
                }
            }

            regions.Add(newRegion);
        }
        else
        {
            _renderQueue.Add(window);
            _renderRegions.Add(
                [
                    region,
                ]
            );
        }
    }

    /// @see WindowRenderer.as::flushRenderQueue
    public void FlushRenderQueue()
    {
        _renderQueue.Clear();
        _renderRegions.Clear();
    }

    /// @see WindowRenderer.as::invalidate
    /// @see WindowRenderer.as lines 401-409 — iterate children in reverse, pass null region
    public void Invalidate(IWindowContext context, Rect2? region)
    {
        IDesktopWindow? desktop = context.GetDesktopWindow();

        if (desktop == null)
        {
            return;
        }

        int n = desktop.numChildren;

        while (n-- > 0)
        {
            IWindow? child = desktop.GetChildAt(n);

            // @see WindowRenderer.as::invalidate — AS3 does not filter by visibility
            if (child != null)
            {
                // @see WindowRenderer.as — AS3 always passes null, causing each child to use its full
                // renderingWidth/renderingHeight as dirty rect inside AddToRenderQueue
                AddToRenderQueue(child, null, Class3655.REDRAW);
            }
        }
    }

    /// @see WindowRenderer.as::getWindowRendererItem
    protected WindowRendererItem GetWindowRendererItem(IWindow window)
    {
        if (!_renderItems.TryGetValue(window, out WindowRendererItem? item))
        {
            item = RegisterRenderable(window);
        }

        return item;
    }

    /// @see WindowRenderer.as::registerRenderable
    public WindowRendererItem RegisterRenderable(IWindow window)
    {
        if (!_renderItems.TryGetValue(window, out WindowRendererItem? item))
        {
            item = new WindowRendererItem(SkinContainer);

            _renderItems[window] = item;

            item.Invalidate(window, Class3655.STATE);
        }

        // Register for dispose events
        if (!window.HasEventListener("WINDOW_DISPOSE_EVENT"))
        {
            window.AddEventListener("WINDOW_DISPOSE_EVENT", WindowDisposedCallback);
        }

        return item;
    }

    /// @see WindowRenderer.as::removeRenderable
    public void RemoveRenderable(IWindow window)
    {
        window.RemoveEventListener("WINDOW_DISPOSE_EVENT", WindowDisposedCallback);

        if (!_renderItems.TryGetValue(window, out WindowRendererItem? item))
        {
            return;
        }

        item.Dispose();
        _renderItems.Remove(window);
    }

    /// @see WindowRenderer.as::windowDisposedCallback
    protected void WindowDisposedCallback(Events.WindowEvent evt, IWindow window)
    {
        RemoveRenderable(window);
    }

    /// @see WindowRenderer.as::getDrawBufferForRenderable
    public Image? GetDrawBufferForRenderable(IWindow window)
    {
        // @see WindowRenderer.as::getDrawBufferForRenderable — AS3 returns existing buffer even if null
        if (_renderItems.TryGetValue(window, out WindowRendererItem? item))
        {
            return item.Buffer;
        }

        item = RegisterRenderable(window);
        item.Invalidate(window, Class3655.REDRAW);

        // @see WindowRenderer.as::getDrawBufferForRenderable — AS3 uses renderingWidth/Height/Rectangle
        TrackedImage tempBuffer = new((int)window.renderingWidth, (int)window.renderingHeight);
        Rect2 region = new(0, 0, window.renderingWidth, window.renderingHeight);

        item.Render(window, Vector2.Zero, region, window.renderingRectangle, tempBuffer.ImageData);
        tempBuffer.Dispose();

        return item.Buffer;
    }

    /// @see WindowRenderer.as::render
    public void Render()
    {
        while (_renderQueue.Count > 0)
        {
            int last = _renderQueue.Count - 1;
            IWindow window = _renderQueue[last];
            List<Rect2> regions = _renderRegions[last];

            _renderQueue.RemoveAt(last);
            _renderRegions.RemoveAt(last);

            if (window.disposed)
            {
                continue;
            }

            Image? drawBuffer = null;

            if (window is IGraphicContextHost host)
            {
                // Godot adaptation: create GC on first render (AS3 Sprite was always present).
                IGraphicContext? gc = host.GetGraphicContext(true);

                // Godot adaptation: wire GC display node into parent's GC hierarchy.
                // In AS3, Flash's display list handles parent-child Sprite relationships
                // automatically. In Godot, we must explicitly parent Node2D nodes.
                if (gc?.DisplayNode != null && gc.DisplayNode.GetParent() == null &&
                    window.parent is IGraphicContextHost parentHost)
                {
                    IGraphicContext? parentGc = parentHost.GetGraphicContext(false);

                    parentGc?.AddChildContext(gc);
                }

                drawBuffer = gc?.FetchDrawBuffer();
            }

            foreach (Rect2 region in regions)
            {
                // @see WindowRenderer.as::render — AS3 uses renderingRectangle for the parent region
                RenderWindowBranch(window, region, window.renderingRectangle, drawBuffer);
            }

            // Godot adaptation: push updated buffer to display texture after rendering.
            if (window is IGraphicContextHost renderedHost)
            {
                renderedHost.GetGraphicContext(false)?.UpdateDisplayTexture();
            }
        }
    }

    /// @see WindowRenderer.as::renderWindowBranch
    private void RenderWindowBranch
    (
        IWindow window,
        Rect2 dirtyRegion,
        Rect2 parentRegion,
        Image? parentBuffer
    )
    {
        // Update graphic context visibility
        IGraphicContext? gc = null;

        if (window is IGraphicContextHost host)
        {
            gc = host.GetGraphicContext(false);

            if (gc != null)
            {
                gc.Visible = window.visible;
            }
        }

        if (!window.visible)
        {
            return;
        }

        // @see WindowRenderer.as::renderWindowBranch — AS3 uses renderingX/Y for draw point
        Vector2 drawPoint = new(window.renderingX, window.renderingY);
        Rect2 clipRegion = new(0, 0, window.renderingWidth, window.renderingHeight);

        if (!GetDrawLocationAndClipRegion(window, dirtyRegion, ref drawPoint, ref clipRegion))
        {
            // Not visible — handle graphic context hiding
            if (window.TestParamFlag(16) || !window.TestParamFlag(0x40000000))
            {
                return;
            }

            if (gc == null && window is IGraphicContextHost host2)
            {
                gc = host2.GetGraphicContext(true);
            }

            if (gc == null)
            {
                return;
            }

            // @see WindowRenderer.as — AS3 uses renderingRectangle
            gc.SetDrawRegion(window.renderingRectangle, false, clipRegion);
            gc.Visible = false;
            return;
        }

        if (window.clipping)
        {
            // @see WindowRenderer.as — AS3 uses renderingRectangle for clipping intersection
            parentRegion = parentRegion.Intersection(window.renderingRectangle);
        }
        parentRegion = new Rect2(
            parentRegion.Position.X - window.x,
            parentRegion.Position.Y - window.y,
            parentRegion.Size.X, parentRegion.Size.Y
        );

        parentBuffer = GetWindowRendererItem(window)
            .Render(
                window, drawPoint, clipRegion, parentRegion, parentBuffer
            );

        // Godot adaptation: push rendered content to Sprite2D texture after each window renders.
        // In AS3, Flash composited Sprite children automatically; here we must push explicitly.
        if (window is IGraphicContextHost renderedWindowHost)
        {
            renderedWindowHost.GetGraphicContext(false)?.UpdateDisplayTexture();
        }

        // Recurse into children
        if (window is not IChildWindowHost containerIface)
        {
            return;
        }

        object? children = containerIface.Children;

        if (children == null)
        {
            return;
        }

        Rect2 childDirtyRegion = dirtyRegion;

        // @see WindowRenderer.as line 527-548 — clipping adjustment
        if (window.clipping)
        {
            float cx = childDirtyRegion.Position.X;
            float cy = childDirtyRegion.Position.Y;
            float cw = childDirtyRegion.Size.X;
            float ch = childDirtyRegion.Size.Y;

            if (cx < 0)
            {
                cw += cx;
                cx = 0;
            }

            if (cy < 0)
            {
                ch += cy;
                cy = 0;
            }

            // @see WindowRenderer.as line 540-547 — compare with width but cap with renderingWidth/Height
            if (cw > window.width)
            {
                cw = window.renderingWidth;
            }

            if (ch > window.height)
            {
                ch = window.renderingHeight;
            }

            childDirtyRegion = new Rect2(cx, cy, cw, ch);
        }

        if (children is IList<IWindow> childList)
        {
            foreach (IWindow child in childList)
            {
                Rect2 childRect = new(child.x, child.y, child.width, child.height);

                if (childRect.Intersects(childDirtyRegion))
                {
                    if (child.TestParamFlag(16))
                    {
                        Rect2 offsetRegion = new(
                            childDirtyRegion.Position.X - child.x,
                            childDirtyRegion.Position.Y - child.y,
                            childDirtyRegion.Size.X, childDirtyRegion.Size.Y
                        );
                        RenderWindowBranch(child, offsetRegion, parentRegion, parentBuffer);
                    }
                    else if (child.TestParamFlag(0x40000000))
                    {
                        // @see WindowRenderer.as line 566 — AS3 calls fetchDrawBuffer() on IWindow
                        // which internally uses GetGraphicContext(true) to ensure GC exists.
                        Image? childBuffer = null;

                        if (child is IGraphicContextHost childHost)
                        {
                            IGraphicContext? childGc = childHost.GetGraphicContext(true);

                            // Godot adaptation: wire child GC display node into parent GC hierarchy
                            if (gc != null && childGc?.DisplayNode != null &&
                                childGc.DisplayNode.GetParent() == null)
                            {
                                gc.AddChildContext(childGc);
                            }

                            childBuffer = childGc?.FetchDrawBuffer();
                        }

                        Rect2 offsetRegion = new(
                            childDirtyRegion.Position.X - child.x,
                            childDirtyRegion.Position.Y - child.y,
                            childDirtyRegion.Size.X, childDirtyRegion.Size.Y
                        );

                        RenderWindowBranch(child, offsetRegion, parentRegion, childBuffer);
                    }
                    else if (child.visible)
                    {
                        if (child is not IGraphicContextHost childHost3 || !childHost3.HasGraphicsContext())
                        {
                            continue;
                        }

                        IGraphicContext? childGc = childHost3.GetGraphicContext(true);

                        if (childGc == null)
                        {
                            continue;
                        }

                        childGc.Visible = true;

                        // Godot adaptation: wire the child GC display node into the parent GC hierarchy
                        if (gc != null && childGc.DisplayNode != null &&
                            childGc.DisplayNode.GetParent() == null)
                        {
                            gc.AddChildContext(childGc);
                        }

                        // Godot adaptation: render the child's visual content (background, skin).
                        // In AS3, Flash rendered these Sprite children automatically via the
                        // display list; in Godot we must drive rendering explicitly.
                        Rect2 branch3Region = new(
                            childDirtyRegion.Position.X - child.x,
                            childDirtyRegion.Position.Y - child.y,
                            childDirtyRegion.Size.X, childDirtyRegion.Size.Y
                        );
                        RenderWindowBranch(child, branch3Region, parentRegion, childGc.FetchDrawBuffer());
                    }
                }
                else if (!childRect.Intersects(parentRegion))
                {
                    if (child is not IGraphicContextHost childHost4 || !childHost4.HasGraphicsContext())
                    {
                        continue;
                    }

                    IGraphicContext? childGc = childHost4.GetGraphicContext(true);

                    if (childGc != null)
                    {
                        childGc.Visible = false;
                    }
                }
            }
        }

    }

    /// @see WindowRenderer.as::areRectanglesCloseEnough
    private static bool AreRectanglesCloseEnough(Rect2 a, Rect2 b, int threshold)
    {
        if (a.Intersects(b))
        {
            return true;
        }

        float hDist = a.Position.X > b.Position.X
            ? a.Position.X - b.End.X
            : b.Position.X - a.End.X;
        float vDist = a.Position.Y > b.Position.Y
            ? a.Position.Y - b.End.Y
            : b.Position.Y - a.End.Y;

        return hDist <= threshold && vDist <= threshold;
    }

    /// @see WindowRenderer.as::getDrawLocationAndClipRegion
    private static bool GetDrawLocationAndClipRegion
    (
        IWindow window,
        Rect2 dirtyRegion,
        ref Vector2 drawPoint,
        ref Rect2 clipRegion
    )
    {
        // @see WindowRenderer.as line 73-76 — uses renderingWidth/renderingHeight
        clipRegion = new Rect2(0, 0, window.renderingWidth, window.renderingHeight);

        if (!window.TestParamFlag(16))
        {
            if (window.parent != null && window.TestParamFlag(0x40000000))
            {
                ChildRectToClippedDrawRegion(window.parent, ref drawPoint, ref clipRegion);
                drawPoint = new Vector2(clipRegion.Position.X, clipRegion.Position.Y);
            }
            else
            {
                drawPoint = Vector2.Zero;
            }
        }
        else if (window.parent != null)
        {
            ChildRectToClippedDrawRegion(window.parent, ref drawPoint, ref clipRegion);
        }
        else
        {
            drawPoint = Vector2.Zero;
        }

        // Clip against dirty region
        if (dirtyRegion.Position.X > clipRegion.Position.X)
        {
            float delta = dirtyRegion.Position.X - clipRegion.Position.X;
            drawPoint = new Vector2(drawPoint.X + delta, drawPoint.Y);
            clipRegion = new Rect2(
                clipRegion.Position.X + delta, clipRegion.Position.Y,
                clipRegion.Size.X - delta, clipRegion.Size.Y
            );
        }

        if (dirtyRegion.Position.Y > clipRegion.Position.Y)
        {
            float delta = dirtyRegion.Position.Y - clipRegion.Position.Y;
            drawPoint = new Vector2(drawPoint.X, drawPoint.Y + delta);
            clipRegion = new Rect2(
                clipRegion.Position.X, clipRegion.Position.Y + delta,
                clipRegion.Size.X, clipRegion.Size.Y - delta
            );
        }

        if (dirtyRegion.End.X < clipRegion.End.X)
        {
            float delta = clipRegion.End.X - dirtyRegion.End.X;
            clipRegion = new Rect2(clipRegion.Position, new Vector2(clipRegion.Size.X - delta, clipRegion.Size.Y));
        }

        if (dirtyRegion.End.Y < clipRegion.End.Y)
        {
            float delta = clipRegion.End.Y - dirtyRegion.End.Y;
            clipRegion = new Rect2(clipRegion.Position, new Vector2(clipRegion.Size.X, clipRegion.Size.Y - delta));
        }

        return clipRegion.Size is { X: > 0, Y: > 0 };
    }

    /// @see WindowRenderer.as::childRectToClippedDrawRegion
    private static bool ChildRectToClippedDrawRegion
    (
        IWindow parent,
        ref Vector2 drawPoint,
        ref Rect2 clipRegion
    )
    {
        if (parent.TestParamFlag(16))
        {
            // @see WindowRenderer.as line 134-135 — uses renderingX/renderingY
            float px = parent.renderingX;
            float py = parent.renderingY;
            drawPoint = new Vector2(drawPoint.X + px, drawPoint.Y + py);

            if (parent.clipping)
            {
                float dpx = drawPoint.X;
                float dpy = drawPoint.Y;
                float crx = clipRegion.Position.X;
                float cry = clipRegion.Position.Y;
                float crw = clipRegion.Size.X;
                float crh = clipRegion.Size.Y;

                if (dpx < px)
                {
                    float d = px - dpx;
                    crx += d;
                    crw -= d;
                    dpx = px;
                }

                if (dpx < 0)
                {
                    crx -= dpx;
                    crw += dpx;
                    dpx = 0;
                }

                if (dpy < py)
                {
                    float d = py - dpy;
                    cry += d;
                    crh -= d;
                    dpy = py;
                }

                if (dpy < 0)
                {
                    cry -= dpy;
                    crh += dpy;
                    dpy = 0;
                }

                // @see WindowRenderer.as line 165-171 — uses renderingWidth/renderingHeight
                if (dpx + crw > px + parent.renderingWidth)
                {
                    crw -= dpx + crw - (px + parent.renderingWidth);
                }

                if (dpy + crh > py + parent.renderingHeight)
                {
                    crh -= dpy + crh - (py + parent.renderingHeight);
                }

                drawPoint = new Vector2(dpx, dpy);
                clipRegion = new Rect2(crx, cry, crw, crh);
            }

            if (parent.parent != null)
            {
                ChildRectToClippedDrawRegion(parent.parent, ref drawPoint, ref clipRegion);
            }
        }
        else if (parent.clipping)
        {
            float dpx = drawPoint.X;
            float dpy = drawPoint.Y;
            float crx = clipRegion.Position.X;
            float cry = clipRegion.Position.Y;
            float crw = clipRegion.Size.X;
            float crh = clipRegion.Size.Y;

            if (dpx < 0)
            {
                crx -= dpx;
                crw += dpx;
                dpx = 0;
            }

            if (dpy < 0)
            {
                cry -= dpy;
                crh += dpy;
                dpy = 0;
            }

            drawPoint = new Vector2(dpx, dpy);
            clipRegion = new Rect2(crx, cry, crw, crh);
        }

        return clipRegion.Size is { X: > 0, Y: > 0 };
    }
}
