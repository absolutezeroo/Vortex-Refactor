// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/WindowModel.as

using System;
using System.Linq;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window;

/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/WindowModel.as
public class WindowModel : IWindow, IDisposable
{
    protected readonly List<IWindow> _children = [];
    protected readonly List<string> _tags = [];
    protected bool _disposed;
    protected IWindow? _parent;

    protected uint _type;
    protected uint _state;
    protected uint _style;
    protected uint _param;
    protected uint _alpha;
    protected uint _color = 0xFFFFFF;
    protected float _blend = 1f;
    protected bool _background;
    protected bool _clipping = true;
    protected bool _visible = true;
    protected uint _mouseThreshold = 10;

    protected float _initialWidth;
    protected float _initialHeight;
    protected float _previousWidth;
    protected float _previousHeight;

    /// @see WindowModel.as::WindowModel
    protected WindowModel() { }

    /// @see WindowModel.as::WindowModel
    protected WindowModel(string param1, Rect2 param2)
    {
        name = param1;
        x = param2.Position.X;
        y = param2.Position.Y;
        width = param2.Size.X;
        height = param2.Size.Y;
        _initialWidth = param2.Size.X;
        _initialHeight = param2.Size.Y;
        _previousWidth = param2.Size.X;
        _previousHeight = param2.Size.Y;
    }

    public int id { get; set; }

    public string name { get; set; } = string.Empty;

    protected string _caption = string.Empty;

    public virtual string caption
    {
        get => _caption;
        set => _caption = value;
    }

    /// @see WindowModel.as — AS3 x/y/width/height are int; truncate to match
    private float _x,
        _y,
        _w,
        _h;

    public virtual float x
    {
        get => _x;
        set => _x = MathF.Truncate(value);
    }

    public virtual float y
    {
        get => _y;
        set => _y = MathF.Truncate(value);
    }

    public virtual float width
    {
        get => _w;
        set => _w = MathF.Truncate(value);
    }

    public virtual float height
    {
        get => _h;
        set => _h = MathF.Truncate(value);
    }

    public virtual bool visible
    {
        get => _visible;
        set => _visible = value;
    }

    public IList<string> tags => _tags;

    public virtual IWindow? parent
    {
        get => _parent;
        set { /* base no-op — overridden by WindowController */ }
    }

    public bool disposed => _disposed;

    /// @see WindowModel.as — IIterable implementation
    public virtual object? Iterator()
    {
        return null;
    }

    public int numChildren => _children.Count;

    /// @see WindowModel.as::type
    public virtual uint type
    {
        get => _type;
        set => _type = value;
    }

    /// @see WindowModel.as::state
    public virtual uint state
    {
        get => _state;
        set => _state = value;
    }

    /// @see WindowModel.as::style
    public virtual uint style
    {
        get => _style;
        set => _style = value;
    }

    /// @see WindowModel.as::param
    public virtual uint param
    {
        get => _param;
        set => _param = value;
    }

    /// @see WindowModel.as::alpha
    public virtual uint alpha
    {
        get => _alpha;
        set => _alpha = value;
    }

    /// @see WindowModel.as::color
    public virtual uint color
    {
        get => _color;
        set => _color = value;
    }

    /// @see WindowModel.as::blend
    public virtual float blend
    {
        get => _blend;
        set => _blend = value;
    }

    /// @see WindowController.as::dynamicStyleColor — base implementation (null)
    public virtual ColorTransform? dynamicStyleColor { get; set; }

    /// @see WindowModel.as::background
    public virtual bool background
    {
        get => _background;
        set => _background = value;
    }

    /// @see WindowModel.as::clipping
    public virtual bool clipping
    {
        get => _clipping;
        set => _clipping = value;
    }

    /// @see WindowModel.as::mouseThreshold — clamped to 255
    public virtual uint mouseThreshold
    {
        get => _mouseThreshold;
        set => _mouseThreshold = Math.Min(value, 255u);
    }

    /// @see WindowModel.as — not stored in model, overridden by controller
    public virtual Action<WindowEvent, IWindow>? procedure { get; set; }

    /// @see WindowModel.as::left
    public float left => x;

    /// @see WindowModel.as::top
    public float top => y;

    /// @see WindowModel.as::right
    public float right => x + width;

    /// @see WindowModel.as::bottom
    public float bottom => y + height;

    /// @see WindowModel.as::rectangle
    public virtual Rect2 rectangle
    {
        get => new(x, y, width, height);
        set
        {
            x = value.Position.X;
            y = value.Position.Y;
            width = value.Size.X;
            height = value.Size.Y;
        }
    }

    /// @see WindowModel.as::position
    public virtual Vector2 position
    {
        get => new(x, y);
        set
        {
            x = value.X;
            y = value.Y;
        }
    }

    /// @see WindowModel.as::_offsetX, _offsetY — rendering offsets
    protected float _offsetX;

    protected float _offsetY;

    /// @see WindowModel.as::etchingPoint — base returns zero, overridable by controllers
    public virtual Vector2 etchingPoint => Vector2.Zero;

    /// @see WindowModel.as::renderingX
    public float renderingX => x + _offsetX + etchingPoint.X;

    /// @see WindowModel.as::renderingY
    public float renderingY => y + _offsetY + etchingPoint.Y;

    /// @see WindowModel.as::renderingWidth
    public float renderingWidth => width + Math.Abs(etchingPoint.X);

    /// @see WindowModel.as::renderingHeight
    public float renderingHeight => height + Math.Abs(etchingPoint.Y);

    /// @see WindowModel.as::renderingRectangle
    public Rect2 renderingRectangle => new(renderingX, renderingY, renderingWidth, renderingHeight);

    /// @see WindowModel.as::dispose — IDisposable implementation
    public virtual void Dispose()
    {
        Destroy();
    }

    /// @see WindowModel.as::dispose
    public virtual bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        // @see WindowModel.as::dispose — set destroyed flag before cleanup
        _state |= 1073741824;
        _disposed = true;

        foreach (IWindow child in _children.ToArray())
        {
            child.Destroy();
        }

        _children.Clear();
        _parent = null;

        return true;
    }

    /// @see WindowModel.as — base does not add children
    public virtual bool AddChild(IWindow param1)
    {
        if (_disposed || param1 == this)
        {
            return false;
        }

        if (_children.Contains(param1))
        {
            return true;
        }

        _children.Add(param1);

        if (param1 is WindowModel model)
        {
            model._parent = this;
        }

        return true;
    }

    public virtual bool RemoveChild(IWindow param1)
    {
        if (!_children.Remove(param1))
        {
            return false;
        }

        if (param1 is WindowModel model)
        {
            model._parent = null;
        }

        return true;
    }

    /// @see WindowModel.as::addChildAt
    public virtual IWindow? AddChildAt(IWindow param1, int param2)
    {
        if (_disposed || param1 == this)
        {
            return null;
        }

        _children.Remove(param1);

        param2 = Math.Clamp(param2, 0, _children.Count);

        _children.Insert(param2, param1);

        if (param1 is WindowModel model)
        {
            model._parent = this;
        }

        return param1;
    }

    public IWindow? GetChildAt(int param1)
    {
        if (param1 < 0 || param1 >= _children.Count)
        {
            return null;
        }

        return _children[param1];
    }

    /// @see WindowModel.as::getChildByID
    public IWindow? GetChildByID(int param1)
    {
        return _children.FirstOrDefault(w => w.id == param1);
    }

    public IWindow? GetChildByName(string param1)
    {
        return _children.FirstOrDefault(w => string.Equals(w.name, param1, StringComparison.Ordinal));
    }

    public IWindow? FindChildByName(string param1)
    {
        IWindow? direct = GetChildByName(param1);

        if (direct != null)
        {
            return direct;
        }

        return _children.Select(child => child.FindChildByName(param1)).OfType<IWindow>().FirstOrDefault();
    }

    /// @see WindowModel.as::getChildByTag
    public IWindow? GetChildByTag(string param1)
    {
        return _children.FirstOrDefault(w => w.tags.Contains(param1));
    }

    /// @see WindowModel.as::findChildByTag
    public IWindow? FindChildByTag(string param1)
    {
        IWindow? direct = GetChildByTag(param1);

        if (direct != null)
        {
            return direct;
        }

        return _children.Select(child => child.FindChildByTag(param1)).OfType<IWindow>().FirstOrDefault();
    }

    /// @see WindowModel.as::getChildIndex
    public int GetChildIndex(IWindow? param1)
    {
        return _children.IndexOf(param1);
    }

    /// @see WindowModel.as::removeChildAt
    public virtual IWindow? RemoveChildAt(int param1)
    {
        if (param1 < 0 || param1 >= _children.Count)
        {
            return null;
        }

        IWindow child = _children[param1];

        _children.RemoveAt(param1);

        if (child is WindowModel model)
        {
            model._parent = null;
        }

        return child;
    }

    /// @see WindowModel.as::setChildIndex
    public virtual void SetChildIndex(IWindow? param1, int param2)
    {
        if (param1 == null)
        {
            return;
        }

        int idx = _children.IndexOf(param1);

        if (idx < 0)
        {
            return;
        }

        _children.RemoveAt(idx);
        param2 = Math.Clamp(param2, 0, _children.Count);
        _children.Insert(param2, param1);
    }

    /// @see WindowModel.as::swapChildren
    public void SwapChildren(IWindow param1, IWindow param2)
    {
        int idx1 = _children.IndexOf(param1);
        int idx2 = _children.IndexOf(param2);

        if (idx1 >= 0 && idx2 >= 0)
        {
            (_children[idx1], _children[idx2]) = (_children[idx2], _children[idx1]);
        }
    }

    /// @see WindowModel.as::swapChildrenAt
    public void SwapChildrenAt(int param1, int param2)
    {
        if (param1 >= 0 && param1 < _children.Count && param2 >= 0 && param2 < _children.Count)
        {
            (_children[param1], _children[param2]) = (_children[param2], _children[param1]);
        }
    }

    /// @see WindowModel.as::groupChildrenWithID
    public uint GroupChildrenWithID(uint param1, IList<IWindow> param2, int param3 = 0)
    {
        uint count = 0u;

        foreach (IWindow child in _children)
        {
            if (child.id == (int)param1)
            {
                param2.Add(child);
                count++;
            }

            if (param3 != 0)
            {
                count += child.GroupChildrenWithID(param1, param2, param3 - 1);
            }
        }

        return count;
    }

    /// @see WindowModel.as::groupChildrenWithTag
    public uint GroupChildrenWithTag(string param1, IList<IWindow> param2, int param3 = 0)
    {
        uint count = 0u;

        foreach (IWindow child in _children)
        {
            if (child.tags.Contains(param1))
            {
                param2.Add(child);
                count++;
            }

            if (param3 != 0)
            {
                count += child.GroupChildrenWithTag(param1, param2, param3 - 1);
            }
        }

        return count;
    }

    /// @see WindowModel.as::center
    public virtual void Center()
    {
        if (_parent == null)
        {
            return;
        }

        x = (_parent.width - width) / 2f;
        y = (_parent.height - height) / 2f;
    }

    /// @see WindowModel.as::offset
    public virtual void Offset(float param1, float param2)
    {
        x += param1;
        y += param2;
    }

    /// @see WindowModel.as::invalidate
    public virtual void Invalidate(Rect2? param1 = null) { }

    /// @see IWindow — default no-op for setRectangle; overridden by WindowController
    public virtual void SetRectangle(float param1, float param2, float param3, float param4)
    {
        x = param1;
        y = param2;
        width = param3;
        height = param4;
    }

    /// @see IWindow — default no-op for scale; overridden by WindowController
    public virtual void Scale(float param1, float param2)
    {
        width += param1;
        height += param2;
    }

    /// @see IWindow — default implementations for coordinate conversion
    public virtual Vector2 GetGlobalPosition()
    {
        float gx = x;
        float gy = y;
        IWindow? current = _parent;

        while (current != null)
        {
            gx += current.x;
            gy += current.y;
            current = current.parent;
        }

        return new Vector2(gx, gy);
    }

    public virtual bool HitTestLocalPoint(Vector2 param1)
    {
        return param1.X >= 0 && param1.X <= width && param1.Y >= 0 && param1.Y <= height;
    }

    public virtual bool HitTestGlobalPoint(Vector2 param1)
    {
        Vector2 pos = GetGlobalPosition();
        return param1.X >= pos.X && param1.X <= pos.X + width &&
               param1.Y >= pos.Y && param1.Y <= pos.Y + height;
    }

    public virtual Vector2 ConvertPointFromGlobalToLocalSpace(Vector2 param1)
    {
        Vector2 global = GetGlobalPosition();

        return new Vector2(param1.X - global.X, param1.Y - global.Y);
    }

    public virtual Vector2 ConvertPointFromLocalToGlobalSpace(Vector2 param1)
    {
        Vector2 global = GetGlobalPosition();

        return new Vector2(param1.X + global.X, param1.Y + global.Y);
    }

    /// @see WindowModel.as::testStateFlag
    public bool TestStateFlag(uint param1, uint param2 = 0)
    {
        if (param2 > 0)
        {
            return ((_state & param2) ^ param1) == 0;
        }

        return (_state & param1) == param1;
    }

    /// @see WindowModel.as::testStyleFlag
    public bool TestStyleFlag(uint param1, uint param2 = 0)
    {
        if (param2 > 0)
        {
            return ((_style & param2) ^ param1) == 0;
        }
        return (_style & param1) == param1;
    }

    /// @see WindowModel.as::testParamFlag
    public bool TestParamFlag(uint param1, uint param2 = 0)
    {
        if (param2 > 0)
        {
            return ((_param & param2) ^ param1) == 0;
        }
        return (_param & param1) == param1;
    }

    /// @see WindowModel.as::setStateFlag
    public virtual void SetStateFlag(uint param1, bool param2 = true)
    {
        if (param2)
        {
            _state |= param1;
        }
        else
        {
            _state &= ~param1;
        }
    }

    /// @see WindowModel.as::getStateFlag
    public bool GetStateFlag(uint param1)
    {
        return (_state & param1) != 0;
    }

    /// @see WindowModel.as::setStyleFlag
    public virtual void SetStyleFlag(uint param1, bool param2 = true)
    {
        if (param2)
        {
            _style |= param1;
        }
        else
        {
            _style &= ~param1;
        }
    }

    /// @see WindowModel.as::getStyleFlag
    public bool GetStyleFlag(uint param1)
    {
        return (_style & param1) != 0;
    }

    /// @see WindowModel.as::setParamFlag
    public virtual void SetParamFlag(uint param1, bool param2 = true)
    {
        if (param2)
        {
            _param |= param1;
        }
        else
        {
            _param &= ~param1;
        }
    }

    /// @see WindowModel.as::getParamFlag
    public bool GetParamFlag(uint param1)
    {
        return (_param & param1) != 0;
    }

    /// @see IWindow — default no-op implementations; overridden by WindowController
    public virtual bool Activate()
    {
        return false;
    }

    public virtual bool Deactivate()
    {
        return false;
    }

    public virtual bool Minimize()
    {
        return false;
    }

    public virtual bool Maximize()
    {
        return false;
    }

    public virtual bool Restore()
    {
        return false;
    }

    public virtual bool Lock()
    {
        return false;
    }

    public virtual bool Unlock()
    {
        return false;
    }

    public virtual bool Enable()
    {
        return false;
    }

    public virtual bool Disable()
    {
        return false;
    }

    public virtual void AddEventListener(string param1, Action<WindowEvent, IWindow> param2, int param3 = 0) { }
    public virtual void RemoveEventListener(string param1, Action<WindowEvent, IWindow> param2) { }
    public virtual bool HasEventListener(string param1)
    {
        return false;
    }

    /// @see WindowController.as::findParentByName
    public IWindow? FindParentByName(string param1)
    {
        IWindow? current = _parent;

        while (current != null)
        {
            if (string.Equals(current.name, param1, StringComparison.Ordinal))
            {
                return current;
            }

            current = current.parent;
        }

        return null;
    }

    /// @see WindowController.as::isEnabled
    public virtual bool IsEnabled()
    {
        return !GetStateFlag(WindowController.STATE_DISABLED);
    }

    /// Sets the parent reference. Used internally by WindowController child management.
    internal void SetParentInternal(IWindow? newParent)
    {
        _parent = newParent;
    }
}
