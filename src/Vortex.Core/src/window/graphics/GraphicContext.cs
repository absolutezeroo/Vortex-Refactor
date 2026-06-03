// @see core/window/graphics/GraphicContext.as

using System;

using Godot;

namespace Vortex.Core.Window.Graphics;

/// <summary>
/// Software rendering graphic context. Replaces AS3 Sprite-based GraphicContext
/// with Node2D wrapping for Godot scene tree display.
/// In AS3, GraphicContext extends Sprite — it IS a display object.
/// Godot adaptation: wraps a Node2D that mirrors its state.
/// </summary>
/// @see core/window/graphics/GraphicContext.as
public class GraphicContext : IGraphicContext, IDisposable
{
    public const uint GC_TYPE_NONE = 0;
    public const uint GC_TYPE_BITMAP = 1;
    public const uint GC_TYPE_TEXTFIELD = 2;
    public const uint GC_TYPE_CONTAINER = 4;
    public const uint GC_TYPE_SHAPE = 8;
    public const uint GC_TYPE_MORPH_SHAPE = 16;
    /// Godot adaptation: empty type for desktop/root contexts that only serve as containers.
    public const uint GC_TYPE_EMPTY = 256;

    private readonly bool _useAlpha;
    private readonly bool _isBitmapType;
    private Rect2 _rectangle;
    private Rect2? _clipRegion;
    private TrackedImage? _drawBuffer;
    private List<IGraphicContext>? _childContexts;
    private bool _visible = true;
    private float _blend = 1.0f;

    /// Godot adaptation: Node2D display hierarchy mirroring AS3 Sprite display list.
    private readonly Node2D _displayNode;
    private Sprite2D? _bitmapSprite;
    private ImageTexture? _displayTexture;
    private Node2D? _childContainerNode;

    public static uint NumGraphicContexts { get; private set; }

    public static long AllocatedByteCount { get; private set; }

    /// @see GraphicContext.as::GraphicContext
    public GraphicContext(string name, uint type, Rect2 rectangle)
    {
        NumGraphicContexts++;
        Name = name;
        _rectangle = rectangle == default ? new Rect2(0, 0, 0, 0) : rectangle;
        _useAlpha = true;
        _isBitmapType = type == GC_TYPE_BITMAP;

        // Godot adaptation: create Node2D hierarchy based on type.
        _displayNode = new Node2D
        {
            Name = SanitizeNodeName(name),
            Position = new Vector2(_rectangle.Position.X, _rectangle.Position.Y),
        };

        if (!_isBitmapType)
        {
            return;
        }

        _bitmapSprite = new Sprite2D
        {
            Name = "BitmapSprite",
            Centered = false,
        };
        _displayNode.AddChild(_bitmapSprite);

        if (_rectangle.Size is { X: > 0, Y: > 0 })
        {
            AllocateDrawBuffer((int)_rectangle.Size.X, (int)_rectangle.Size.Y);
        }
    }

    public string Name { get; }

    public bool Disposed { get; private set; }

    /// Godot adaptation: exposes the Node2D for scene tree attachment.
    public Node2D? DisplayNode => Disposed ? null : _displayNode;

    public bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            _displayNode.Visible = value;
        }
    }

    public float Blend
    {
        get => _blend;
        set
        {
            _blend = value;
            _displayNode.Modulate = new Color(1, 1, 1, value);
        }
    }

    /// Godot adaptation: color modulate on the display node (maps to Flash ColorTransform)
    public Color Modulate
    {
        get => _displayNode.Modulate;
        set => _displayNode.Modulate = value;
    }

    public bool Mouse { get; set; }

    public object?[]? Filters { get; set; }

    public int NumChildContexts => _childContexts?.Count ?? 0;

    /// @see GraphicContext.as::offSet — only update display position, NOT _rectangle
    public void Offset(Vector2 point)
    {
        _displayNode.Position = point;
    }

    /// @see GraphicContext.as::dispose
    public void Dispose()
    {
        if (Disposed)
        {
            return;
        }

        while (NumChildContexts > 0)
        {
            RemoveChildContextAt(0);
        }

        if (_isBitmapType)
        {
            ReleaseDrawBuffer();
        }

        _displayTexture?.Dispose();
        _displayTexture = null;
        _bitmapSprite = null;
        _childContainerNode = null;
        _childContexts = null;
        Disposed = true;
        NumGraphicContexts--;

        if (GodotObject.IsInstanceValid(_displayNode))
        {
            _displayNode.QueueFree();
        }
    }

    public override string ToString()
    {
        return $"[object GraphicContext name=\"{Name}\"]";
    }

    /// @see GraphicContext.as::getDrawRegion
    public Rect2 GetDrawRegion()
    {
        return _rectangle;
    }

    /// @see GraphicContext.as::setDrawRegion — stores clipRegion and configures Godot clip mode
    public Image? SetDrawRegion(Rect2 region, bool reallocate, Rect2? clipRegion)
    {
        Image? buffer = null;

        if (region.Size.X < 1 || region.Size.Y < 1)
        {
            return null;
        }

        if (_isBitmapType && reallocate)
        {
            buffer = AllocateDrawBuffer((int)region.Size.X, (int)region.Size.Y);
        }

        _rectangle = region;
        _clipRegion = clipRegion;
        _displayNode.Position = new Vector2(region.Position.X, region.Position.Y);

        // @see GraphicContext.as — apply clip mask via Godot's ClipChildren
        if (clipRegion.HasValue)
        {
            _displayNode.ClipChildren = CanvasItem.ClipChildrenMode.Only;
        }
        else
        {
            _displayNode.ClipChildren = CanvasItem.ClipChildrenMode.Disabled;
        }

        return buffer;
    }

    /// @see GraphicContext.as::fetchDrawBuffer
    public Image? FetchDrawBuffer()
    {
        return _isBitmapType ? _drawBuffer?.ImageData : null;
    }

    /// @see GraphicContext.as::showRedrawRegion
    public void ShowRedrawRegion(Rect2? region)
    {
        // Debug visualization — no-op in software renderer
    }

    /// Godot adaptation: pushes the current Image buffer to the ImageTexture for display.
    public void UpdateDisplayTexture()
    {
        if (!_isBitmapType || _drawBuffer?.ImageData == null || _bitmapSprite == null)
        {
            return;
        }

        if (_displayTexture == null)
        {
            _displayTexture = ImageTexture.CreateFromImage(_drawBuffer.ImageData);
            _bitmapSprite.Texture = _displayTexture;
        }
        else
        {
            _displayTexture.Update(_drawBuffer.ImageData);
        }
    }

    /// @see GraphicContext.as::allocateDrawBuffer
    protected Image? AllocateDrawBuffer(int width, int height)
    {
        if (!_isBitmapType)
        {
            return null;
        }

        if (_drawBuffer != null)
        {
            if (_drawBuffer.Width == width && _drawBuffer.Height == height)
            {
                return _drawBuffer.ImageData;
            }

            AllocatedByteCount -= (long)_drawBuffer.Width * _drawBuffer.Height * 4;
            _drawBuffer.Dispose();
            _drawBuffer = null;
        }

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        _drawBuffer = new TrackedImage(width, height, _useAlpha, 0x00FFFFFF);
        AllocatedByteCount += (long)width * height * 4;

        // Godot adaptation: create/update ImageTexture for the new buffer.
        if (_bitmapSprite == null)
        {
            return _drawBuffer.ImageData;
        }

        _displayTexture?.Dispose();
        _displayTexture = ImageTexture.CreateFromImage(_drawBuffer.ImageData);
        _bitmapSprite.Texture = _displayTexture;

        return _drawBuffer.ImageData;
    }

    /// @see GraphicContext.as::releaseDrawBuffer
    protected void ReleaseDrawBuffer()
    {
        if (!_isBitmapType || _drawBuffer == null)
        {
            return;
        }

        AllocatedByteCount -= (long)_drawBuffer.Width * _drawBuffer.Height * 4;
        _drawBuffer.Dispose();
        _drawBuffer = null;

        _displayTexture?.Dispose();
        _displayTexture = null;

        if (_bitmapSprite != null)
        {
            _bitmapSprite.Texture = null;
        }
    }

    private Node2D EnsureChildContainer()
    {
        if (_childContainerNode != null)
        {
            return _childContainerNode;
        }

        _childContainerNode = new Node2D
        {
            Name = "Children",
        };
        _displayNode.AddChild(_childContainerNode);

        return _childContainerNode;
    }

    private List<IGraphicContext> EnsureChildContexts()
    {
        _childContexts ??= [];

        return _childContexts;
    }

    /// @see GraphicContext.as::addChildContext
    public IGraphicContext AddChildContext(IGraphicContext child)
    {
        EnsureChildContexts().Add(child);

        if (child.DisplayNode != null)
        {
            EnsureChildContainer().AddChild(child.DisplayNode);
        }

        return child;
    }

    /// @see GraphicContext.as::addChildContextAt
    public IGraphicContext AddChildContextAt(IGraphicContext child, int index)
    {
        EnsureChildContexts().Insert(index, child);

        if (child.DisplayNode == null)
        {
            return child;
        }

        Node2D container = EnsureChildContainer();

        container.AddChild(child.DisplayNode);
        container.MoveChild(child.DisplayNode, index);

        return child;
    }

    /// @see GraphicContext.as::getChildContextAt
    public IGraphicContext GetChildContextAt(int index)
    {
        return EnsureChildContexts()[index];
    }

    /// @see GraphicContext.as::getChildContextByName
    public IGraphicContext? GetChildContextByName(string name)
    {
        if (_childContexts == null)
        {
            return null;
        }

        foreach (IGraphicContext child in _childContexts)
        {
            if (child is GraphicContext gc && gc.Name == name)
            {
                return child;
            }
        }

        return null;
    }

    /// @see GraphicContext.as::getChildContextIndex
    public int GetChildContextIndex(IGraphicContext child)
    {
        return EnsureChildContexts().IndexOf(child);
    }

    /// @see GraphicContext.as::removeChildContext
    public IGraphicContext RemoveChildContext(IGraphicContext child)
    {
        EnsureChildContexts().Remove(child);

        if (child.DisplayNode != null && _childContainerNode != null &&
            child.DisplayNode.GetParent() == _childContainerNode)
        {
            _childContainerNode.RemoveChild(child.DisplayNode);
        }

        return child;
    }

    /// @see GraphicContext.as::removeChildContextAt
    public IGraphicContext? RemoveChildContextAt(int index)
    {
        List<IGraphicContext> children = EnsureChildContexts();

        if (index < 0 || index >= children.Count)
        {
            return null;
        }

        IGraphicContext child = children[index];
        children.RemoveAt(index);

        if (child.DisplayNode != null && _childContainerNode != null &&
            child.DisplayNode.GetParent() == _childContainerNode)
        {
            _childContainerNode.RemoveChild(child.DisplayNode);
        }

        return child;
    }

    /// @see GraphicContext.as::setChildContextIndex
    public void SetChildContextIndex(IGraphicContext child, int index)
    {
        List<IGraphicContext> children = EnsureChildContexts();
        int current = children.IndexOf(child);

        if (current < 0)
        {
            throw new Exception("Provided display object is not a child of this!");
        }

        if (current == index)
        {
            return;
        }

        children.RemoveAt(current);
        children.Insert(index, child);

        if (child.DisplayNode != null && _childContainerNode != null)
        {
            _childContainerNode.MoveChild(child.DisplayNode, index);
        }
    }

    /// @see GraphicContext.as::swapChildContexts
    public void SwapChildContexts(IGraphicContext child1, IGraphicContext child2)
    {
        List<IGraphicContext> children = EnsureChildContexts();
        int idx1 = children.IndexOf(child1);
        int idx2 = children.IndexOf(child2);

        if (idx1 < 0 || idx2 < 0)
        {
            return;
        }

        children[idx1] = child2;
        children[idx2] = child1;

        if (_childContainerNode == null)
        {
            return;
        }

        if (child1.DisplayNode != null)
        {
            _childContainerNode.MoveChild(child1.DisplayNode, idx2);
        }

        if (child2.DisplayNode != null)
        {
            _childContainerNode.MoveChild(child2.DisplayNode, idx1);
        }
    }

    /// @see GraphicContext.as::swapChildContextsAt
    public void SwapChildContextsAt(int index1, int index2)
    {
        List<IGraphicContext> children = EnsureChildContexts();

        (children[index1], children[index2]) = (children[index2], children[index1]);

        if (_childContainerNode == null)
        {
            return;
        }

        if (children[index1].DisplayNode != null)
        {
            _childContainerNode.MoveChild(children[index1].DisplayNode, index1);
        }

        if (children[index2].DisplayNode != null)
        {
            _childContainerNode.MoveChild(children[index2].DisplayNode, index2);
        }
    }

    /// Godot adaptation: sanitize name for use as a Godot node name.
    private static string SanitizeNodeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "GC";
        }

        // Replace characters invalid in Godot node names
        return name.Replace("/", "_").Replace(".", "_").Replace(":", "_");
    }
}
