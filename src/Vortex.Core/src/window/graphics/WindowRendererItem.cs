// @see core/window/graphics/WindowRendererItem.as

using System;

using Godot;

using Vortex.Core.Window.Graphics.Renderer;

namespace Vortex.Core.Window.Graphics;

/// @see core/window/graphics/WindowRendererItem.as
public class WindowRendererItem : IDisposable
{
    protected const uint RENDER_TYPE_NULL = 0;
    protected const uint RENDER_TYPE_SKIN = 1;
    protected const uint RENDER_TYPE_FILL = 2;

    private ISkinContainer? _skinContainer;
    private TrackedImage? _buffer;
    private bool _refresh;
    private uint _lastState = uint.MaxValue;
    private uint _currentState;

    /// @see WindowRendererItem.as::WindowRendererItem
    public WindowRendererItem(ISkinContainer skinContainer)
    {
        _skinContainer = skinContainer;
    }

    public bool Disposed { get; private set; }

    public Image? Buffer => _buffer?.ImageData;

    /// @see WindowRendererItem.as::dispose
    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        Disposed = true;

        // @see WindowRendererItem.as::dispose — AS3 nulls skin container reference
        _skinContainer = null;

        _buffer?.Dispose();
        _buffer = null;
    }

    /// @see WindowRendererItem.as::purge
    public static void Purge() { }

    /// @see WindowRendererItem.as::render
    public Image? Render
    (
        IWindow window,
        Vector2 drawPoint,
        Rect2 clipRegion,
        Rect2 parentRegion,
        Image? parentBuffer
    )
    {
        uint renderType = window.background ? RENDER_TYPE_FILL : RENDER_TYPE_NULL;

        ISkinRenderer? skinRenderer = _skinContainer?.GetSkinRendererByTypeAndStyle(window.type, window.style);

        if (skinRenderer != null && skinRenderer.IsStateDrawable(_currentState))
        {
            renderType = RENDER_TYPE_SKIN;
        }

        // @see WindowRendererItem.as — AS3 uses renderingWidth/renderingHeight (includes etching offsets)
        int renderWidth = Math.Max((int)window.renderingWidth, 1);
        int renderHeight = Math.Max((int)window.renderingHeight, 1);
        bool needsAlloc = true;

        if (renderType != RENDER_TYPE_NULL)
        {
            if (_buffer == null || _buffer.Width != renderWidth || _buffer.Height != renderHeight)
            {
                _buffer?.Dispose();
                _buffer = new TrackedImage(renderWidth, renderHeight, true, window.color);
                _refresh = true;
                needsAlloc = false;
            }
        }

        // Handle graphic context
        IGraphicContext? graphicContext = null;

        if (window is IGraphicContextHost host)
        {
            graphicContext = host.GetGraphicContext(false);

            if (graphicContext != null)
            {
                if (!graphicContext.Visible)
                {
                    graphicContext.Visible = true;
                }

                bool clipping = window.TestParamFlag(0x40000000);
                // @see WindowRendererItem.as — AS3 uses renderingRectangle (includes etching offsets)
                Rect2 renderRect = window.renderingRectangle;
                bool softwareRender = !window.TestParamFlag(16);
                // Godot adaptation: pass local position so the GC display node is placed
                // relative to its parent GC, not at the screen-space renderingRectangle origin.
                // renderingX/Y are screen-space; subtract parent's renderingX/Y for local coords.
                float localX = window.parent != null
                    ? window.renderingX - window.parent.renderingX
                    : window.renderingX;
                float localY = window.parent != null
                    ? window.renderingY - window.parent.renderingY
                    : window.renderingY;
                Image? newBuffer = graphicContext.SetDrawRegion(
                    renderRect, softwareRender,
                    clipping ? parentRegion : null,
                    new Vector2(localX, localY)
                );

                if (newBuffer != null)
                {
                    parentBuffer = newBuffer;

                    _refresh = true;
                }
            }
        }

        bool softwareMode = !window.TestParamFlag(16);

        if (renderType != RENDER_TYPE_NULL)
        {
            if (parentBuffer != null)
            {
                switch (renderType)
                {
                    case RENDER_TYPE_SKIN:
                        {
                            if (_refresh)
                            {
                                if (softwareMode)
                                {
                                    // Clear parent region
                                    ClearRegion(parentBuffer, clipRegion);
                                }
                                _refresh = false;

                                if (needsAlloc && _buffer != null)
                                {
                                    FillBuffer(_buffer.ImageData, window.color);
                                }

                                if (_buffer != null && skinRenderer != null)
                                {
                                    skinRenderer.Draw(
                                        window, _buffer.ImageData,
                                        new Rect2(0, 0, _buffer.Width, _buffer.Height),
                                        _currentState, false
                                    );
                                }
                            }

                            // Composite buffer onto parent
                            if (_buffer != null && parentBuffer != null)
                            {
                                BlitToParent(
                                    parentBuffer, _buffer.ImageData,
                                    (int)drawPoint.X, (int)drawPoint.Y,
                                    clipRegion, window.blend, softwareMode
                                );
                            }
                            break;
                        }
                    case RENDER_TYPE_FILL when !softwareMode && _buffer != null:
                        FillBuffer(_buffer.ImageData, window.color);
                        BlitToParent(
                            parentBuffer, _buffer.ImageData,
                            (int)drawPoint.X, (int)drawPoint.Y,
                            clipRegion, window.blend, false
                        );
                        break;
                    case RENDER_TYPE_FILL:
                        {
                            FillRegionOnParent(
                                parentBuffer,
                                (int)drawPoint.X, (int)drawPoint.Y,
                                (int)clipRegion.Size.X, (int)clipRegion.Size.Y,
                                window.color
                            );

                            if (graphicContext != null)
                            {
                                graphicContext.Blend = window.blend;
                            }
                            break;
                        }
                }
            }
        }
        else if (_refresh && softwareMode)
        {
            _refresh = false;
            if (parentBuffer != null)
            {
                ClearRegion(parentBuffer, clipRegion);
            }
        }

        // @see WindowRenderer.as — UpdateDisplayTexture is called by WindowRenderer.Render after full branch render,
        // not per-item. Removed duplicate call here.

        _lastState = _currentState;
        return parentBuffer;
    }

    /// @see WindowRendererItem.as::testForStateChange
    public bool TestForStateChange(IWindow window)
    {
        return _skinContainer != null && _skinContainer.GetTheActualState(window.type, window.style, window.state) != _lastState;
    }

    /// @see WindowRendererItem.as::invalidate
    public bool Invalidate(IWindow window, uint flags)
    {
        bool needsRender = false;

        switch (flags)
        {
            case Class3655.REDRAW:
                _refresh = true;
                needsRender = true;
                break;
            case Class3655.RESIZE:
                _refresh = true;
                needsRender = true;
                break;
            case Class3655.RELOCATE:
                if (window.TestParamFlag(16))
                {
                    needsRender = true;
                }
                else if (window is IGraphicContextHost host)
                {
                    IGraphicContext? gc = host.GetGraphicContext(true);

                    if (gc != null)
                    {
                        // @see WindowRendererItem.as — AS3 uses renderingRectangle
                        float relocLocalX = window.parent != null
                            ? window.renderingX - window.parent.renderingX
                            : window.renderingX;
                        float relocLocalY = window.parent != null
                            ? window.renderingY - window.parent.renderingY
                            : window.renderingY;
                        gc.SetDrawRegion(window.renderingRectangle, false, null,
                            new Vector2(relocLocalX, relocLocalY));
                        if (!gc.Visible)
                        {
                            needsRender = true;
                        }
                    }
                }
                break;
            case Class3655.STATE:
                _currentState = _skinContainer?.GetTheActualState(window.type, window.style, window.state) ?? 0;

                if (_currentState != _lastState)
                {
                    _refresh = true;
                    needsRender = true;
                }
                break;
            case Class3655.BLEND:
                if (window.TestParamFlag(16))
                {
                    _refresh = true;
                    needsRender = true;
                }
                else if (window is IGraphicContextHost blendHost)
                {
                    IGraphicContext? gc = blendHost.GetGraphicContext(true);

                    if (gc != null)
                    {
                        gc.Blend = window.blend;
                    }
                }
                break;
            case Class3655.CASCADE:
                needsRender = true;
                break;
        }

        return needsRender;
    }

    /// @see WindowRendererItem.as::drawRect (debug border)
    private static void DrawRect(Image buffer, Rect2 region, uint color)
    {
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);
        Color c = new(r / 255f, g / 255f, b / 255f, 1f);
        int left = (int)region.Position.X;
        int top = (int)region.Position.Y;
        int right = (int)region.End.X;
        int bottom = (int)region.End.Y;
        int w = buffer.GetWidth();
        int h = buffer.GetHeight();

        for (int x = left;
             x < right && x < w;
             x++)
        {
            if (top >= 0 && top < h)
            {
                buffer.SetPixel(x, top, c);
            }
            if (bottom - 1 >= 0 && bottom - 1 < h)
            {
                buffer.SetPixel(x, bottom - 1, c);
            }
        }

        for (int y = top;
             y < bottom && y < h;
             y++)
        {
            if (left >= 0 && left < w)
            {
                buffer.SetPixel(left, y, c);
            }
            if (right - 1 >= 0 && right - 1 < w)
            {
                buffer.SetPixel(right - 1, y, c);
            }
        }
    }

    /// Godot adaptation: uses Image.FillRect instead of per-pixel clear.
    private static void ClearRegion(Image buffer, Rect2 region)
    {
        int x0 = Math.Max(0, (int)region.Position.X);
        int y0 = Math.Max(0, (int)region.Position.Y);
        int x1 = Math.Min(buffer.GetWidth(), (int)region.End.X);
        int y1 = Math.Min(buffer.GetHeight(), (int)region.End.Y);

        if (x1 <= x0 || y1 <= y0)
        {
            return;
        }

        buffer.FillRect(new Rect2I(x0, y0, x1 - x0, y1 - y0), new Color(0, 0, 0, 0));
    }

    private static void FillBuffer(Image buffer, uint color)
    {
        // @see AS3 BitmapData.fillRect — alpha byte from color is used as-is.
        // When color has no alpha (e.g. 0xFFFFFF), alpha = 0x00 → transparent fill.
        byte a = (byte)((color >> 24) & 0xFF);
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);

        buffer.Fill(new Color(r / 255f, g / 255f, b / 255f, a / 255f));
    }

    /// Godot adaptation: uses Image.BlendRect for the common case (blend >= 1).
    /// For blend < 1: creates a temporary copy with alpha pre-multiplied by blend factor,
    /// then uses BlendRect — avoids expensive per-pixel GetPixel/SetPixel loops.
    /// @see AS3 copyPixels(source, rect, point, null, null, mergeAlpha=true)
    private static void BlitToParent
    (
        Image parent,
        Image source,
        int destX,
        int destY,
        Rect2 clipRegion,
        float blend,
        bool softwareMode
    )
    {
        int srcX = (int)clipRegion.Position.X;
        int srcY = (int)clipRegion.Position.Y;
        int w = (int)clipRegion.Size.X;
        int h = (int)clipRegion.Size.Y;

        if (w <= 0 || h <= 0)
        {
            return;
        }

        if (blend < 1f && !softwareMode)
        {
            // Godot optimization: create a temp image with the clip region, scale its alpha
            // channel in bulk via byte[] access, then BlendRect onto parent.
            Image temp = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
            temp.BlitRect(source, new Rect2I(srcX, srcY, w, h), Vector2I.Zero);

            byte[] data = temp.GetData();
            int blendByte = (int)(blend * 256f);

            for (int i = 3; i < data.Length; i += 4)
            {
                data[i] = (byte)Math.Clamp((data[i] * blendByte) >> 8, 0, 255);
            }

            temp.SetData(w, h, false, Image.Format.Rgba8, data);
            parent.BlendRect(temp, new Rect2I(0, 0, w, h), new Vector2I(destX, destY));
        }
        else
        {
            // Godot BlendRect handles alpha compositing natively (mergeAlpha=true equivalent).
            parent.BlendRect(source, new Rect2I(srcX, srcY, w, h), new Vector2I(destX, destY));
        }
    }

    /// Godot adaptation: uses Image.FillRect instead of per-pixel fill.
    private static void FillRegionOnParent(Image parent, int x, int y, int w, int h, uint color)
    {
        byte a = (byte)((color >> 24) & 0xFF);
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);

        Color c = new(r / 255f, g / 255f, b / 255f, a / 255f);

        int pw = parent.GetWidth();
        int ph = parent.GetHeight();
        int x0 = Math.Max(0, x);
        int y0 = Math.Max(0, y);
        int x1 = Math.Min(x + w, pw);
        int y1 = Math.Min(y + h, ph);

        if (x1 <= x0 || y1 <= y0)
        {
            return;
        }

        parent.FillRect(new Rect2I(x0, y0, x1 - x0, y1 - y0), c);
    }
}
