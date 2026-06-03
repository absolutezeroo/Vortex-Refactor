// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/WindowController.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window.Dynamicstyle;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Services;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window;

/// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/window/WindowController.as
public class WindowController : WindowModel, IWindowContainer, IGraphicContextHost, IChildWindowHost
{
    public const string TAG_EXCLUDE = "_EXCLUDE";
    public const string TAG_INTERNAL = "_INTERNAL";
    public const string TAG_COLORIZE = "_COLORIZE";
    public const string TAG_IGNORE_INHERITED_STYLE = "_IGNORE_INHERITED_STYLE";

    /// @see WindowController.as — state flag constants (const_479=64 for locked)
    public const uint STATE_ACTIVATED = 1;
    public const uint STATE_FOCUSED = 2;
    public const uint STATE_LOCKED = 64;
    public const uint STATE_SELECTED = 8;
    public const uint STATE_DISABLED = 32;

    private static uint _instanceCounter;

    protected WindowEventDispatcher? _eventDispatcher;
    protected Action<WindowEvent, IWindow>? _procedure;
    protected new WindowController? _parent;
    protected IWindowContext? _context;
    internal string _dynamicStyle = "";
    private ColorTransform? _dynamicStyleColor;
    /// @see WindowController.as::var_3157 — cached DynamicStyle object
    private DynamicStyle? _cachedDynamicStyle;
    private readonly uint _instanceId;
    private bool _hasHadBackground;
    private bool _graphicsContextSetUp;

    /// @see WindowController.as::var_1650 — graphic context for this window
    protected IGraphicContext? _var1650;

    /// @see WindowController.as::_fillColor — actual color used for rendering (alpha conditional on _background)
    private uint _fillColor = 0xFFFFFF;

    /// @see WindowController.as::_alphaColor — stores alpha bits (high byte) separately
    private uint _alphaColor;

    private Rect2 _previousRect;
    protected WindowRectLimits? _limits;

    /// @see WindowController.as::var_2115 — stores parent's rectangle at time child was added
    private Rect2 _parentRectOnAdd;

    /// @see WindowController.as::WindowController
    public WindowController() : base()
    {
        _instanceId = _instanceCounter++;
        _eventDispatcher = new WindowEventDispatcher(this);
    }

    /// @see WindowController.as::WindowController
    public WindowController(string param1, Rect2 param2) : base(param1, param2)
    {
        _instanceId = _instanceCounter++;
        _eventDispatcher = new WindowEventDispatcher(this);
    }

    /// @see WindowController.as::WindowController (full AS3 12-param signature)
    public WindowController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0,
        string param12 = ""
    ) : base(param1, param6)
    {
        _instanceId = _instanceCounter++;
        _type = param2;
        _style = param3;
        _param = param4;
        _context = param5;
        _procedure = param8;
        _dynamicStyle = param12;
        id = (int)param11;
        _eventDispatcher = new WindowEventDispatcher(this);

        if (param10 != null)
        {
            foreach (string tag in param10)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    _tags.Add(tag.Trim());
                }
            }
        }

        // @see WindowController.as — ensure graphic context exists before layout parsing
        _var1650 ??= GetGraphicContext(!TestParamFlag(16));
        _graphicsContextSetUp = true;

        // @see WindowController.as — fetch and inject window layout XML from SkinContainer
        if (param5 is WindowContext)
        {
            XElement? layoutXml = WindowContext.GetRenderer()
                                               ?.SkinContainer
                                               .GetWindowLayoutByTypeAndStyle(param2, param3);

            if (layoutXml != null)
            {
                // @see WindowController.as — ALWAYS set dimensions to layout size before parsing.
                // Children are created with the parent at layout size. Then setRectangle()
                // transitions to the actual size, firing WE_RESIZED which cascades
                // WE_PARENT_RESIZED to children so they can MOVE/STRETCH correctly.
                float layoutWidth = (float?)layoutXml.Attribute("width") ?? 10f;
                float layoutHeight = (float?)layoutXml.Attribute("height") ?? 10f;

                base.x = 0;
                base.y = 0;
                base.width = layoutWidth;
                base.height = layoutHeight;
                _initialWidth = layoutWidth;
                _initialHeight = layoutHeight;
                _previousWidth = layoutWidth;
                _previousHeight = layoutHeight;

                // @see WindowController.as — parse layout XML to create child structure
                param5.GetWindowParser().ParseAndConstruct(layoutXml, this, null);

                // @see WindowController.as — transition from layout size to actual size via SetRectangle.
                // This fires WE_RESIZED → WE_PARENT_RESIZED, making MOVE/STRETCH children respond.
                // Mask REFLECT_RESIZE_TO_PARENT flags during this transition.
                if (param6.Size is { X: > 0, Y: > 0 })
                {
                    uint savedParam = _param;
                    _param &= ~(uint)12582912; // mask reflect flags
                    SetRectangle(param6.Position.X, param6.Position.Y, param6.Size.X, param6.Size.Y);
                    _param = savedParam;
                }
            }

            // @see WindowController.as — apply default attributes from SkinContainer
            DefaultAttStruct? defaults = WindowContext.GetRenderer()
                                                      ?.SkinContainer
                                                      .GetDefaultAttributesByTypeAndStyle(param2, param3);

            if (defaults != null)
            {
                _blend = defaults.Blend;
                _mouseThreshold = defaults.Threshold;

                if (_background != defaults.Background)
                {
                    background = defaults.Background;
                }

                if (_fillColor != defaults.Color)
                {
                    color = defaults.Color;
                }

                // @see WindowController.as — limits already enforced in SetRectangle (lines 894-900)
            }
        }

        // @see WindowController.as — parse and apply XML properties
        if (param9 is { Count: > 0 })
        {
            PropertyStruct[] parsed = XmlPropertyArrayParser.Parse(param9);
            ApplyProperties(parsed);
        }

        // @see WindowController.as — attach to parent after layout + defaults
        if (param7 == null)
        {
            return;
        }

        _parent = param7 as WindowController;

        param7.AddChild(this);

        // @see WindowController.as — store parent's rect at time child was added (var_2115)
        if (_parent != null)
        {
            _parentRectOnAdd = new Rect2(_parent.x, _parent.y, _parent.width, _parent.height);
        }

        // @see WindowController.as — invalidate after attaching to parent
        if (_var1650 != null)
        {
            _context?.Invalidate(this, new Rect2(0, 0, base.width, base.height), Class3655.STATE);
        }
    }

    /// @see WindowController.as::get procedure — walks parent hierarchy if null
    public override Action<WindowEvent, IWindow>? procedure
    {
        get
        {
            if (_procedure != null)
            {
                return _procedure;
            }

            if (_parent != null)
            {
                return _parent.procedure;
            }

            return null;
        }
        set => _procedure = value;
    }

    /// @see WindowController.as::set type — invalidate on change
    public override uint type
    {
        get => _type;
        set
        {
            _type = value;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set style — invalidate on change
    public override uint style
    {
        get => _style;
        set
        {
            _style = value;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set clipping — invalidate on change
    public override bool clipping
    {
        get => _clipping;
        set
        {
            _clipping = value;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set state — invalidate + renderDynamicStyle on change
    public override uint state
    {
        get => _state;
        set
        {
            uint old = _state;
            _state = value;

            if (_state == old)
            {
                return;
            }

            RenderDynamicStyle();
            _context?.Invalidate(this, null, Class3655.STATE);
        }
    }

    /// @see WindowController.as::set caption — normalizes + invalidates
    public override string caption
    {
        get => _caption;
        set
        {
            string val = value ?? "";

            if (val == _caption)
            {
                return;
            }

            _caption = val;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set background
    /// AS3: _fillColor = _background ? _fillColor | _alphaColor : _fillColor & 0x00FFFFFF
    public override bool background
    {
        get => _background;
        set
        {
            _background = value;
            _hasHadBackground |= value;
            _fillColor = _background ? _fillColor | _alphaColor : _fillColor & 0x00FFFFFF;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set alpha — applies _alphaColor, updates _fillColor, invalidates
    public override uint alpha
    {
        get => _alpha;
        set
        {
            _alpha = value;
            _alphaColor = value << 24;
            _fillColor = _background ? (_fillColor & 0x00FFFFFF) | _alphaColor : _fillColor & 0x00FFFFFF;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set color
    /// AS3: _alphaColor = param1 & 0xFF000000; _fillColor = _background ? param1 : param1 & 0x00FFFFFF
    public override uint color
    {
        get => _fillColor;
        set
        {
            _alphaColor = value & 0xFF000000;
            _fillColor = _background ? value : value & 0x00FFFFFF;
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set blend — clamps to [0,1], invalidates with null rect
    public override float blend
    {
        get => _blend;
        set
        {
            value = Math.Clamp(value, 0f, 1f);

            if (Math.Abs(_blend - value) < float.Epsilon)
            {
                return;
            }

            _blend = value;

            _context?.Invalidate(this, null, Class3655.BLEND);
        }
    }

    /// @see WindowController.as::set x — routes through SetRectangle for limits, anchors, containment
    public override float x
    {
        get => base.x;
        set
        {
            if (Math.Abs(base.x - value) < float.Epsilon)
            {
                return;
            }

            SetRectangle(value, base.y, base.width, base.height);
        }
    }

    /// @see WindowController.as::set y — routes through SetRectangle for limits, anchors, containment
    public override float y
    {
        get => base.y;
        set
        {
            if (Math.Abs(base.y - value) < float.Epsilon)
            {
                return;
            }

            SetRectangle(base.x, value, base.width, base.height);
        }
    }

    /// @see WindowController.as::set width — routes through SetRectangle for limits, anchors, containment
    public override float width
    {
        get => base.width;
        set
        {
            if (Math.Abs(base.width - value) < float.Epsilon)
            {
                return;
            }

            SetRectangle(base.x, base.y, value, base.height);
        }
    }

    /// @see WindowController.as::set height — routes through SetRectangle for limits, anchors, containment
    public override float height
    {
        get => base.height;
        set
        {
            if (Math.Abs(base.height - value) < float.Epsilon)
            {
                return;
            }

            SetRectangle(base.x, base.y, base.width, value);
        }
    }

    /// @see WindowController.as::set visible (lines 400-415)
    public override bool visible
    {
        get => _visible;
        set
        {
            if (_visible == value)
            {
                return;
            }

            _visible = value;

            // @see WindowController.as line 406-408 — propagate hide to GC
            if (_var1650 != null && !value)
            {
                _var1650.Visible = false;
            }

            // @see WindowController.as line 410 — invalidate for redraw
            _context?.Invalidate(this, null, Class3655.REDRAW);

            // @see WindowController.as line 411-412 — dispatch through update()
            WindowEvent evt = new(WindowEvent.WE_CHILD_VISIBILITY, this, this);
            Update(this, evt);
        }
    }

    /// @see WindowController.as::getChildUnderPoint
    /// @see WindowController.as::getChildUnderPoint — recursive + visibility check
    public IWindow? GetChildUnderPoint(Vector2 param1)
    {
        if (!_visible)
        {
            return null;
        }

        for (int i = _children.Count - 1;
             i >= 0;
             i--)
        {
            IWindow child = _children[i];

            if (!child.visible)
            {
                continue;
            }

            // Convert to child-local coordinates and recurse
            Vector2 localPoint = new(param1.X - child.x, param1.Y - child.y);

            if (child is WindowController childCtrl)
            {
                IWindow? deepChild = childCtrl.GetChildUnderPoint(localPoint);

                if (deepChild != null)
                {
                    return deepChild;
                }
            }

            if (param1.X >= child.x && param1.X <= child.x + child.width &&
                param1.Y >= child.y && param1.Y <= child.y + child.height)
            {
                return child;
            }
        }
        return null;
    }

    /// @see WindowController.as::groupChildrenUnderPoint — self-inclusion + recursion + clipping
    public void GroupChildrenUnderPoint(Vector2 param1, IList<IWindow> param2)
    {
        // @see WindowController.as — add self if point is within bounds
        if (param1.X >= 0 && param1.X <= base.width &&
            param1.Y >= 0 && param1.Y <= base.height)
        {
            param2.Add(this);
        }

        for (int i = _children.Count - 1;
             i >= 0;
             i--)
        {
            IWindow child = _children[i];

            if (!child.visible)
            {
                continue;
            }

            Vector2 localPoint = new(param1.X - child.x, param1.Y - child.y);

            bool inBounds = param1.X >= child.x && param1.X <= child.x + child.width &&
                            param1.Y >= child.y && param1.Y <= child.y + child.height;

            if (inBounds)
            {
                param2.Add(child);

                // Recurse into child
                if (child is WindowController childCtrl)
                {
                    childCtrl.GroupChildrenUnderPoint(localPoint, param2);
                }
            }
            else if (!_clipping && child is WindowController childCtrl2)
            {
                // @see WindowController.as — when not clipping, recurse even outside bounds
                childCtrl2.GroupChildrenUnderPoint(localPoint, param2);
            }
        }
    }

    /// @see WindowController.as::groupParameterFilteredChildrenUnderPoint
    public virtual void GroupParameterFilteredChildrenUnderPoint(
        Vector2 param1, IList<IWindow> param2, uint param3 = 0)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            IWindow child = _children[i];

            if (!child.visible)
            {
                continue;
            }

            if (param3 != 0 && (child.param & param3) == 0)
            {
                continue;
            }

            if (param1.X >= child.x && param1.X <= child.x + child.width &&
                param1.Y >= child.y && param1.Y <= child.y + child.height)
            {
                param2.Add(child);
            }

            // @see WindowController.as — recurse into children with local-space offset
            if (child is WindowController childCtrl)
            {
                Vector2 localPoint = new(param1.X - child.x, param1.Y - child.y);
                childCtrl.GroupParameterFilteredChildrenUnderPoint(localPoint, param2, param3);
            }
        }
    }

    /// @see WindowController.as::getGraphicContext
    public virtual IGraphicContext? GetGraphicContext(bool create)
    {
        if (_var1650 != null)
        {
            return _var1650;
        }

        if (!create)
        {
            return null;
        }

        _var1650 = new GraphicContext(
            name, GraphicContext.GC_TYPE_BITMAP,
            new Rect2(base.x, base.y, base.width, base.height)
        )
        {
            Visible = _visible,
        };

        return _var1650;
    }

    /// @see WindowController.as::hasGraphicsContext
    public virtual bool HasGraphicsContext()
    {
        return _var1650 != null || !TestParamFlag(16);
    }

    /// @see WindowController.as::fetchDrawBuffer
    public virtual Image? FetchDrawBuffer()
    {
        if (!TestParamFlag(16))
        {
            return GetGraphicContext(true)?.FetchDrawBuffer();
        }

        // Flag 16: use parent's graphic context buffer
        if (_parent is IGraphicContextHost parentHost)
        {
            return parentHost.GetGraphicContext(true)?.FetchDrawBuffer();
        }

        return null;
    }

    /// @see WindowController.as — IClass3402 implementation, exposes children for RenderWindowBranch
    public object? Children => _children;

    /// @see WindowController.as::destroy — AS3 sentinel is var_149 == 0x40000000
    /// destroy() fires WE_DESTROY/WE_DESTROYED events, then calls dispose().
    /// Subclasses override Destroy() to add cleanup before calling base.Destroy().
    public override bool Destroy()
    {
        if (_state == 0x40000000)
        {
            return true;
        }

        _state = 0x40000000;

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_DESTROY);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        // @see WindowController.as::destroy — fire WE_DESTROYED then call dispose()
        WindowEvent destroyedEvt = new(WindowEvent.WE_DESTROYED, this, null, false);
        NotifyEventListeners(destroyedEvt);
        Update(this, destroyedEvt);

        DisposeCleanup();
        return true;
    }

    /// @see WindowController.as::dispose — AS3 dispose does cleanup without destroy events.
    /// Godot adaptation: Dispose() enters through Destroy() chain to ensure subclass
    /// Destroy() overrides get called (where subclass cleanup lives), but skips
    /// WE_DESTROY/WE_DESTROYED events by checking _disposing flag.
    private bool _disposing;

    public override void Dispose()
    {
        if (_disposed || _disposing)
        {
            return;
        }

        _disposing = true;
        // Run through Destroy() to pick up subclass Destroy() override cleanup
        // Sentinel _param = 0x40000000 is set in Destroy, which also fires events.
        // But since AS3 dispose() doesn't fire events, we call DisposeCleanup directly.
        DisposeCleanup();
    }

    /// @see WindowController.as::dispose — actual cleanup logic
    private void DisposeCleanup()
    {
        if (_disposed)
        {
            return;
        }

        // @see WindowController.as line 709 — deactivate if root window and activated
        if (_context != null)
        {
            if (_parent == null || !TestParamFlag(16))
            {
                if (GetStateFlag(STATE_ACTIVATED))
                {
                    Deactivate();
                }
            }
        }

        // @see WindowController.as line 717-722 — dispose children in reverse order (AS3 calls dispose, not destroy)
        if (_children.Count > 0)
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                IWindow child = _children[i];

                if (child is IDisposable disposableChild)
                {
                    disposableChild.Dispose();
                }
                else
                {
                    child.Destroy();
                }
            }
        }

        _children.Clear();

        // @see WindowController.as line 724 — unlink parent
        _parent?.RemoveChild(this);

        // @see WindowController.as line 727-734 — dispatch WindowDisposeEvent, dispose event dispatcher
        if (_eventDispatcher != null)
        {
            WindowDisposeEvent disposeEvt = new(this);
            NotifyEventListeners(disposeEvt);

            _eventDispatcher.Dispose();
            _eventDispatcher = null;
        }

        // @see WindowController.as line 736-740 — dispose graphic context
        if (_var1650 != null)
        {
            _var1650.Dispose();
            _var1650 = null;
        }

        _procedure = null;
        _parent = null;
        _context = null;
        _disposed = true;
    }

    /// @see WindowController.as::activate
    public override bool Activate()
    {
        if (_disposed)
        {
            return false;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_ACTIVATE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_ACTIVATED);
        DispatchSimple(WindowEvent.WE_ACTIVATED);

        return true;
    }

    /// @see WindowController.as::deactivate
    public override bool Deactivate()
    {
        if (_disposed)
        {
            return false;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_DEACTIVATE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_ACTIVATED, false);
        DispatchSimple(WindowEvent.WE_DEACTIVATED);

        return true;
    }

    /// @see WindowController.as::focus
    public bool Focus()
    {
        if (_disposed)
        {
            return false;
        }

        if (GetStateFlag(STATE_FOCUSED))
        {
            return true;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_FOCUS);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_FOCUSED);
        DispatchSimple(WindowEvent.WE_FOCUSED);

        return true;
    }

    /// @see WindowController.as::unfocus
    public bool Unfocus()
    {
        if (_disposed)
        {
            return false;
        }

        if (!GetStateFlag(STATE_FOCUSED))
        {
            return true;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_UNFOCUS);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_FOCUSED, false);
        DispatchSimple(WindowEvent.WE_UNFOCUSED);

        return true;
    }

    /// @see WindowController.as::enable
    public override bool Enable()
    {
        if (_disposed)
        {
            return false;
        }

        if (!GetStateFlag(STATE_DISABLED))
        {
            return true;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_ENABLE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_DISABLED, false);
        DispatchSimple(WindowEvent.WE_ENABLED);

        return true;
    }

    /// @see WindowController.as::disable
    public override bool Disable()
    {
        if (_disposed)
        {
            return false;
        }

        if (GetStateFlag(STATE_DISABLED))
        {
            return true;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_DISABLE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_DISABLED);
        DispatchSimple(WindowEvent.WE_DISABLED);

        return true;
    }

    /// @see WindowController.as::lock
    public override bool Lock()
    {
        if (_disposed)
        {
            return false;
        }

        if (GetStateFlag(STATE_LOCKED))
        {
            return true;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_LOCK);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_LOCKED);
        DispatchSimple(WindowEvent.WE_LOCKED);

        return true;
    }

    /// @see WindowController.as::unlock
    public override bool Unlock()
    {
        if (_disposed)
        {
            return false;
        }

        if (!GetStateFlag(STATE_LOCKED))
        {
            return true;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_UNLOCK);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_LOCKED, false);
        DispatchSimple(WindowEvent.WE_UNLOCKED);

        return true;
    }

    /// @see WindowController.as::minimize — AS3 is state-only, no rect save/restore
    public override bool Minimize()
    {
        if (_disposed)
        {
            return false;
        }

        if (GetStateFlag(STATE_LOCKED))
        {
            return false;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_MINIMIZE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_LOCKED);
        DispatchSimple(WindowEvent.WE_MINIMIZED);

        return true;
    }

    /// @see WindowController.as::maximize — AS3 is state-only, no rect save/restore
    public override bool Maximize()
    {
        if (_disposed)
        {
            return false;
        }

        if (GetStateFlag(STATE_LOCKED))
        {
            return false;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_MAXIMIZE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_LOCKED);
        DispatchSimple(WindowEvent.WE_MAXIMIZED);

        return true;
    }

    /// @see WindowController.as::restore — AS3 is state-only, no rect save/restore
    public override bool Restore()
    {
        if (_disposed)
        {
            return false;
        }

        WindowEvent? evt = DispatchCancelable(WindowEvent.WE_RESTORE);

        if (evt != null && evt.IsWindowOperationPrevented())
        {
            return false;
        }

        SetStateFlag(STATE_LOCKED, false);
        DispatchSimple(WindowEvent.WE_RESTORED);

        return true;
    }

    /// @see WindowController.as::addChild
    public override bool AddChild(IWindow param1)
    {
        if (_disposed || param1 == this || param1.disposed)
        {
            return false;
        }

        if (_children.Contains(param1))
        {
            return true;
        }

        param1.parent?.RemoveChild(param1);

        _children.Add(param1);

        SetChildParent(param1, this);
        SetupChildGraphicsContext(param1);
        DispatchToChild(param1, WindowEvent.WE_PARENT_ADDED);
        DispatchChildEvent(WindowEvent.WE_CHILD_ADDED, param1);

        return true;
    }

    /// @see WindowController.as::addChildAt
    public override IWindow? AddChildAt(IWindow param1, int param2)
    {
        if (_disposed || param1 == this || param1.disposed)
        {
            return null;
        }

        param1.parent?.RemoveChild(param1);

        _children.Remove(param1);

        param2 = Math.Clamp(param2, 0, _children.Count);

        _children.Insert(param2, param1);

        SetChildParent(param1, this);
        SetupChildGraphicsContext(param1);

        DispatchToChild(param1, WindowEvent.WE_PARENT_ADDED);
        DispatchChildEvent(WindowEvent.WE_CHILD_ADDED, param1);

        return param1;
    }

    /// @see WindowController.as::removeChild — WE_PARENT_REMOVED fires BEFORE WE_CHILD_REMOVED
    public override bool RemoveChild(IWindow param1)
    {
        if (!_children.Remove(param1))
        {
            return false;
        }

        TeardownChildGraphicsContext(param1);
        DispatchToChild(param1, WindowEvent.WE_PARENT_REMOVED);
        SetChildParent(param1, null);
        DispatchChildEvent(WindowEvent.WE_CHILD_REMOVED, param1);

        return true;
    }

    /// @see WindowController.as::removeChildAt — WE_PARENT_REMOVED fires BEFORE WE_CHILD_REMOVED
    public override IWindow? RemoveChildAt(int param1)
    {
        if (param1 < 0 || param1 >= _children.Count)
        {
            return null;
        }

        IWindow child = _children[param1];

        _children.RemoveAt(param1);

        TeardownChildGraphicsContext(child);
        DispatchToChild(child, WindowEvent.WE_PARENT_REMOVED);
        SetChildParent(child, null);
        DispatchChildEvent(WindowEvent.WE_CHILD_REMOVED, child);

        return child;
    }

    /// @see WindowController.as::setChildIndex — reorder children list + GC display nodes
    public override void SetChildIndex(IWindow? param1, int param2)
    {
        base.SetChildIndex(param1, param2);

        // @see WindowController.as — reorder GC display nodes to match children order
        if (param1 is not IGraphicContextHost childHost)
        {
            return;
        }

        IGraphicContext? childGc = childHost.GetGraphicContext(false);

        if (childGc?.DisplayNode != null && _var1650?.DisplayNode != null)
        {
            _var1650.DisplayNode.MoveChild(childGc.DisplayNode, param2);
        }
    }

    /// @see WindowController.as::center — routes through SetRectangle
    public override void Center()
    {
        if (_parent == null)
        {
            return;
        }

        float newX = (_parent.width - base.width) / 2f;
        float newY = (_parent.height - base.height) / 2f;

        SetRectangle(newX, newY, base.width, base.height);
    }

    /// @see WindowController.as::offset — routes through SetRectangle for limits/anchors/containment
    public override void Offset(float param1, float param2)
    {
        SetRectangle(base.x + param1, base.y + param2, base.width, base.height);
    }

    /// @see WindowController.as::addEventListener
    public override void AddEventListener(string param1, Action<WindowEvent, IWindow> param2, int param3 = 0)
    {
        _eventDispatcher?.AddEventListener(param1, param2, param3);
    }

    /// @see WindowController.as::removeEventListener
    public override void RemoveEventListener(string param1, Action<WindowEvent, IWindow> param2)
    {
        _eventDispatcher?.RemoveEventListener(param1, param2);
    }

    /// @see WindowController.as::hasEventListener
    public override bool HasEventListener(string param1)
    {
        return _eventDispatcher?.HasEventListener(param1) ?? false;
    }

    /// @see WindowController.as::limits
    public WindowRectLimits? limits
    {
        get => _limits;
        set => _limits = value;
    }

    /// @see WindowController.as::filters
    public List<object>? filters { get; set; }

    /// @see WindowController.as::context
    public IWindowContext? context => _context;

    /// @see WindowController.as::dynamicStyle — clears cache, re-renders, invalidates
    public string dynamicStyle
    {
        get => _dynamicStyle;
        set
        {
            _dynamicStyle = value;
            _cachedDynamicStyle = null;
            RenderDynamicStyle();
            _context?.Invalidate(this, null, Class3655.REDRAW);
        }
    }

    /// @see WindowController.as::set parent — reparent via RemoveChild/AddChild
    public override IWindow? parent
    {
        get => _parent;
        set
        {
            if (ReferenceEquals(_parent, value))
            {
                return;
            }

            IWindow? oldParent = _parent;
            oldParent?.RemoveChild(this);
            value?.AddChild(this);
        }
    }

    /// @see WindowController.as::dynamicStyleColor (var_4180)
    public override ColorTransform? dynamicStyleColor
    {
        get => _dynamicStyleColor;
        set => _dynamicStyleColor = value;
    }

    /// @see WindowController.as::setRectangle — lines 776-883
    public virtual void SetRectangle(float param1, float param2, float param3, float param4)
    {
        if (_disposed)
        {
            return;
        }

        // Apply limits enforcement
        if (_limits != null)
        {
            if (param3 < _limits.MinWidth && _limits.MinWidth > int.MinValue)
            {
                param3 = _limits.MinWidth;
            }
            if (param3 > _limits.MaxWidth && _limits.MaxWidth < int.MaxValue)
            {
                param3 = _limits.MaxWidth;
            }
            if (param4 < _limits.MinHeight && _limits.MinHeight > int.MinValue)
            {
                param4 = _limits.MinHeight;
            }
            if (param4 > _limits.MaxHeight && _limits.MaxHeight < int.MaxValue)
            {
                param4 = _limits.MaxHeight;
            }
        }

        // @see WindowController.as lines 789-811 — anchor adjustment only when resizing without independent relocation
        bool anchorRelocated = Math.Abs(base.x - param1) > float.Epsilon || Math.Abs(base.y - param2) > float.Epsilon;
        bool anchorResized = Math.Abs(base.width - param3) > float.Epsilon || Math.Abs(base.height - param4) > float.Epsilon;

        if (anchorResized && !anchorRelocated)
        {
            // Horizontal: 786432 = both bits (524288|262144) → center. 262144 alone → right-anchor.
            uint hFlag = _param & 786432u;

            switch (hFlag)
            {
                // ANCHOR_CENTER_H: adjust by half delta
                case 786432u:
                    param1 -= (param3 - base.width) / 2f;
                    break;
                // ANCHOR_RIGHT: adjust by full delta
                case 262144u:
                    param1 -= param3 - base.width;
                    break;
            }

            // Vertical: 3145728 = both bits (2097152|1048576) → center. 1048576 alone → bottom-anchor.
            uint vFlag = _param & 3145728u;

            switch (vFlag)
            {
                // ANCHOR_CENTER_V: adjust by half delta
                case 3145728u:
                    param2 -= (param4 - base.height) / 2f;
                    break;
                // ANCHOR_BOTTOM: adjust by full delta
                case 1048576u:
                    param2 -= param4 - base.height;
                    break;
            }
        }

        // Parent containment (param flag 32)
        if (TestParamFlag(32) && _parent != null)
        {
            // @see WindowController.as — when only resizing (not relocating), clamp size to parent
            if (!anchorRelocated && anchorResized)
            {
                if (param3 > _parent.width)
                {
                    param3 = _parent.width;
                }

                if (param4 > _parent.height)
                {
                    param4 = _parent.height;
                }
            }

            if (param1 < 0)
            {
                param1 = 0;
            }

            if (param2 < 0)
            {
                param2 = 0;
            }

            if (param1 + param3 > _parent.width)
            {
                param1 = _parent.width - param3;
            }

            if (param2 + param4 > _parent.height)
            {
                param2 = _parent.height - param4;
            }
        }

        bool relocated = Math.Abs(base.x - param1) > float.Epsilon || Math.Abs(base.y - param2) > float.Epsilon;
        bool resized = Math.Abs(base.width - param3) > float.Epsilon || Math.Abs(base.height - param4) > float.Epsilon;

        if (!relocated && !resized)
        {
            return;
        }

        // @see WindowController.as — partial cancellation: relocate and resize are independently cancelable
        if (relocated)
        {
            WindowEvent relocateEvt = new(WindowEvent.WE_RELOCATE, this, null, true);
            NotifyEventListeners(relocateEvt);
            if (relocateEvt.IsWindowOperationPrevented())
            {
                relocated = false;
            }
        }

        if (resized)
        {
            WindowEvent resizeEvt = new(WindowEvent.WE_RESIZE, this, null, true);
            NotifyEventListeners(resizeEvt);
            if (resizeEvt.IsWindowOperationPrevented())
            {
                resized = false;
            }
        }

        if (!relocated && !resized)
        {
            return;
        }

        // Apply only non-cancelled dimensions
        _previousRect = new Rect2(base.x, base.y, base.width, base.height);
        _previousWidth = base.width;
        _previousHeight = base.height;

        if (relocated)
        {
            base.x = param1;
            base.y = param2;
        }

        if (resized)
        {
            base.width = param3;
            base.height = param4;
        }

        // Post-events
        if (relocated)
        {
            DispatchSimple(WindowEvent.WE_RELOCATED);
        }

        if (resized)
        {
            DispatchSimple(WindowEvent.WE_RESIZED);
        }
    }

    /// @see WindowController.as::scale
    public virtual void Scale(float param1, float param2)
    {
        if (_disposed)
        {
            return;
        }

        float newWidth = base.width + param1;
        float newHeight = base.height + param2;

        if (newWidth < 1)
        {
            newWidth = 1;
        }

        if (newHeight < 1)
        {
            newHeight = 1;
        }

        SetRectangle(base.x, base.y, newWidth, newHeight);
    }

    /// @see WindowController.as::update (lines 997-1258) — base event handler for all window events
    /// In AS3 this is the PRIMARY event handler: all events flow through update() first,
    /// which handles invalidation, child notification, accommodation, and parent propagation.
    public virtual bool Update(WindowController param1, WindowEvent param2)
    {
        // @see WindowController.as lines 999-1014 — preamble: call procedure + dispatch before switch
        if (!TestParamFlag(9))
        {
            _procedure?.Invoke(param2, this);

            if (_disposed)
            {
                return true;
            }

            if (!param2.IsWindowOperationPrevented())
            {
                if (HasEventListener(param2.type))
                {
                    _eventDispatcher?.DispatchEvent(param2);
                }
            }

            if (_disposed)
            {
                return true;
            }
        }

        // @see WindowController.as lines 1029-1112 — WindowMouseEvent handling block
        if (param2 is WindowMouseEvent)
        {
            switch (param2.type)
            {
                case WindowMouseEvent.DOWN:
                    {
                        // @see WindowController.as line 1034 — activate + preventDefault if successful
                        if (Activate())
                        {
                            if (param2.cancelable)
                            {
                                param2.PreventDefault();
                            }
                        }

                        if (_disposed)
                        {
                            return true;
                        }

                        // @see WindowController.as line 1045 — set pressed state
                        SetStateFlag(16, true);

                        // @see WindowController.as line 1046-1048 — begin mouse listener
                        if (_context is WindowContext wctx && wctx.GetWindowServices() is ServiceManager services)
                        {
                            WindowMouseListener? listener = services.Listener;
                            if (listener != null)
                            {
                                listener.Begin(this);
                                listener.EventTypes.Add(WindowMouseEvent.UP);
                                listener.AreaLimit = 3;
                            }

                            // @see WindowController.as line 1049-1060 — drag service
                            if (TestParamFlag(257))
                            {
                                WindowController? loc6 = this;

                                while (loc6 != null)
                                {
                                    if (loc6.TestParamFlag(32768))
                                    {
                                        services.Dragger?.Begin(loc6);
                                        break;
                                    }

                                    loc6 = loc6._parent;
                                }
                            }

                            // @see WindowController.as line 1062-1074 — scale service
                            if ((_param & 12288) > 0)
                            {
                                WindowController? loc6 = this;

                                while (loc6 != null)
                                {
                                    if (loc6.TestParamFlag(65536))
                                    {
                                        services.Scaler?.Begin(loc6, _param & 12288);
                                        break;
                                    }

                                    loc6 = loc6._parent;
                                }
                            }
                        }

                        break;
                    }

                case WindowMouseEvent.UP:
                    {
                        // @see WindowController.as line 1077-1089
                        if (GetStateFlag(16))
                        {
                            SetStateFlag(16, false);
                        }

                        if (_context is WindowContext wctxUp && wctxUp.GetWindowServices() is ServiceManager servicesUp)
                        {
                            servicesUp.Listener?.End(this);

                            if (TestParamFlag(32768))
                            {
                                servicesUp.Dragger?.End(this);
                            }

                            if (TestParamFlag(65536))
                            {
                                servicesUp.Scaler?.End(this);
                            }
                        }

                        break;
                    }

                case WindowMouseEvent.OUT:
                    {
                        // @see WindowController.as line 1091-1099 — clear hover and pressed
                        if (GetStateFlag(4))
                        {
                            SetStateFlag(4, false);
                        }

                        if (GetStateFlag(16))
                        {
                            SetStateFlag(16, false);
                        }

                        break;
                    }

                case WindowMouseEvent.OVER:
                    {
                        // @see WindowController.as line 1101-1105 — set hover state
                        if (!GetStateFlag(4))
                        {
                            SetStateFlag(4, true);
                        }

                        break;
                    }

                case WindowMouseEvent.WHEEL:
                    // @see WindowController.as line 1109-1110 — stop propagation
                    return false;
            }
        }

        switch (param2.type)
        {
            case WindowEvent.WE_RESIZED:
                {
                    // @see WindowController.as::update WE_RESIZED — only when event originates from this window
                    if (param1 != this)
                    {
                        return true;
                    }

                    // @see WindowController.as — invalidation rect is union of old + new bounds
                    float minX = Math.Min(0, _previousRect.Position.X - base.x);
                    float minY = Math.Min(0, _previousRect.Position.Y - base.y);
                    float maxR = Math.Max(base.width, _previousRect.Position.X + _previousRect.Size.X - base.x);
                    float maxB = Math.Max(base.height, _previousRect.Position.Y + _previousRect.Size.Y - base.y);
                    Rect2 unionRect = new(minX, minY, maxR - minX, maxB - minY);

                    _context?.Invalidate(this, unionRect, Class3655.RESIZE);
                    NotifyChildren(new WindowEvent(WindowEvent.WE_PARENT_RESIZED, this, null));

                    if (TestParamFlag(192, 192) || TestParamFlag(3072, 3072))
                    {
                        UpdateScaleRelativeToParent();
                    }

                    if (_parent is not WindowController parentCtrl)
                    {
                        return true;
                    }

                    // @see WindowController.as — REFLECT_RESIZE_TO_PARENT (flags 4194304/8388608) BEFORE WE_CHILD_RESIZED
                    // AS3 saves/clears/restores var_837 (reflect flags) around the reflect block
                    uint savedReflect = _param & 12582912u;
                    _param &= ~12582912u;

                    if ((savedReflect & 4194304u) != 0 && _parent != null)
                    {
                        float deltaW = base.width - _previousRect.Size.X;
                        _parent.width += deltaW;
                    }

                    if ((savedReflect & 8388608u) != 0 && _parent != null)
                    {
                        float deltaH = base.height - _previousRect.Size.Y;
                        _parent.height += deltaH;
                    }

                    _param |= savedReflect;

                    WindowEvent childEvt = new(WindowEvent.WE_CHILD_RESIZED, parentCtrl, this);
                    parentCtrl.Update(this, childEvt);

                    return true;
                }

            case WindowEvent.WE_RELOCATED:
                {
                    // @see WindowController.as::update WE_RELOCATED — only when event originates from this window
                    if (param1 != this)
                    {
                        return true;
                    }

                    _context?.Invalidate(this, new Rect2(0, 0, base.width, base.height), Class3655.RELOCATE);
                    NotifyChildren(new WindowEvent(WindowEvent.WE_PARENT_RELOCATED, this, null));

                    if (_parent is not WindowController parentCtrl)
                    {
                        return true;
                    }

                    WindowEvent childEvt = new(WindowEvent.WE_CHILD_RELOCATED, parentCtrl, this);
                    parentCtrl.Update(this, childEvt);

                    return true;
                }

            case WindowEvent.WE_ACTIVATED:
                {
                    // @see WindowController.as::update WE_ACTIVATED — only when event originates from this window
                    if (param1 != this)
                    {
                        return true;
                    }

                    NotifyChildren(new WindowEvent(WindowEvent.WE_PARENT_ACTIVATED, this, null));

                    if (_parent is not WindowController parentCtrl)
                    {
                        return true;
                    }

                    WindowEvent childEvt = new(WindowEvent.WE_CHILD_ACTIVATED, parentCtrl, this);
                    parentCtrl.Update(this, childEvt);

                    return true;
                }

            case WindowEvent.WE_CHILD_ACTIVATED:
                {
                    // @see WindowController.as::update WE_CHILD_ACTIVATED — cascade activation up the tree
                    Activate();

                    return true;
                }

            case WindowEvent.WE_PARENT_ADDED:
                {
                    // @see WindowController.as::update WE_PARENT_ADDED — scale relative to parent, invalidate
                    // @see WindowController.as — testParamFlag(192, 192) checks BOTH bits (center mode)
                    if (TestParamFlag(192, 192) || TestParamFlag(3072, 3072))
                    {
                        UpdateScaleRelativeToParent();
                    }

                    _context?.Invalidate(this, new Rect2(0, 0, base.width, base.height), Class3655.REDRAW);

                    return true;
                }

            case WindowEvent.WE_PARENT_RESIZED:
                {
                    // @see WindowController.as::update WE_PARENT_RESIZED — scale relative to parent
                    UpdateScaleRelativeToParent();
                    // @see WindowController.as line 1202 — getRegionProperties writes PREVIOUS rect to var_2115
                    // The parent's previous rect was already stored in _parentRectOnAdd at add time.
                    // After scaling, update to current parent rect for next delta calculation.
                    if (_parent != null)
                    {
                        _parentRectOnAdd = new Rect2(_parent.x, _parent.y, _parent.width, _parent.height);
                    }

                    return true;
                }

            case WindowEvent.WE_CHILD_ADDED:
                {
                    // @see WindowController.as::update WE_CHILD_ADDED — accommodate, renderDynamicStyle
                    if (TestParamFlag(147456))
                    {
                        ScaleToAccommodateChildren();
                    }
                    else if (TestParamFlag(131072))
                    {
                        ExpandToAccommodateChild(this, param2.related);
                    }

                    // @see WindowController.as line 1214 — always call renderDynamicStyle after accommodation
                    RenderDynamicStyle();

                    return true;
                }

            case WindowEvent.WE_CHILD_REMOVED:
                {
                    // @see WindowController.as::update WE_CHILD_REMOVED — re-accommodate
                    if (TestParamFlag(147456))
                    {
                        ScaleToAccommodateChildren();
                    }

                    return true;
                }

            case WindowEvent.WE_CHILD_RESIZED:
                {
                    // @see WindowController.as::update WE_CHILD_RESIZED — accommodate
                    if (TestParamFlag(147456))
                    {
                        ScaleToAccommodateChildren();
                    }
                    else if (TestParamFlag(131072))
                    {
                        ExpandToAccommodateChild(this, param2.related);
                    }

                    return true;
                }

            case WindowEvent.WE_CHILD_RELOCATED:
                {
                    // @see WindowController.as::update WE_CHILD_RELOCATED — accommodate
                    if (TestParamFlag(147456))
                    {
                        ScaleToAccommodateChildren();
                    }
                    else if (TestParamFlag(131072))
                    {
                        ExpandToAccommodateChild(this, param2.related);
                    }

                    return true;
                }

            case WindowEvent.WE_CHILD_VISIBILITY:
                {
                    // @see WindowController.as::update WE_CHILD_VISIBILITY — forward to parent
                    if (param1 != this || _parent is not WindowController parentCtrl)
                    {
                        return true;
                    }

                    WindowEvent childEvt = new(WindowEvent.WE_CHILD_VISIBILITY, parentCtrl, this);
                    parentCtrl.Update(this, childEvt);

                    return true;
                }
        }

        return false;
    }

    /// @see WindowController.as::expandToAccommodateChild (static)
    /// Expands a window to fit a child that extends beyond its boundaries.
    public static void ExpandToAccommodateChild(WindowController param1, IWindow? param2)
    {
        if (param2 == null)
        {
            return;
        }

        float newX = param1.x;
        float newY = param1.y;
        float newW = param1.width;
        float newH = param1.height;
        float shiftX = 0;
        float shiftY = 0;

        // @see WindowController.as — if child extends left
        if (param2.x < 0)
        {
            shiftX = -param2.x;
            newW += shiftX;
            newX -= shiftX;
        }

        // @see WindowController.as — if child extends right
        if (param2.x + param2.width > newW)
        {
            newW = param2.x + param2.width;
        }

        // @see WindowController.as — if child extends top
        if (param2.y < 0)
        {
            shiftY = -param2.y;
            newH += shiftY;
            newY -= shiftY;
        }

        // @see WindowController.as — if child extends bottom
        if (param2.y + param2.height > newH)
        {
            newH = param2.y + param2.height;
        }

        if (Math.Abs(newW - param1.width) < float.Epsilon &&
            Math.Abs(newH - param1.height) < float.Epsilon)
        {
            return;
        }

        // @see WindowController.as — save and disable accommodate flags to prevent recursion
        uint savedParam = param1._param;
        param1._param &= ~(uint)(131072 | 147456);

        // @see WindowController.as — offset all children if parent shifted
        if (shiftX > 0 || shiftY > 0)
        {
            for (int i = 0;
                 i < param1.numChildren;
                 i++)
            {
                IWindow? child = param1.GetChildAt(i);
                child?.Offset(shiftX, shiftY);
            }
        }

        param1.SetRectangle(newX, newY, newW, newH);
        param1._param = savedParam;
    }

    /// @see WindowController.as::resizeToAccommodateChildren (static)
    /// Resizes a window to tightly fit all visible children.
    public static void ResizeToAccommodateChildren(WindowController param1)
    {
        if (param1.numChildren == 0)
        {
            return;
        }

        float minX = float.MaxValue,
            minY = float.MaxValue;
        float maxRight = 0,
            maxBottom = 0;
        bool hasVisible = false;

        for (int i = 0;
             i < param1.numChildren;
             i++)
        {
            IWindow? child = param1.GetChildAt(i);
            if (child is not { visible: true })
            {
                continue;
            }

            hasVisible = true;

            if (child.x < minX)
            {
                minX = child.x;
            }
            if (child.y < minY)
            {
                minY = child.y;
            }

            float childRight = child.x + child.width;
            float childBottom = child.y + child.height;

            if (childRight > maxRight)
            {
                maxRight = childRight;
            }
            if (childBottom > maxBottom)
            {
                maxBottom = childBottom;
            }
        }

        if (!hasVisible)
        {
            return;
        }

        float newW = maxRight;
        float newH = maxBottom;

        if (Math.Abs(newW - param1.width) < float.Epsilon &&
            Math.Abs(newH - param1.height) < float.Epsilon &&
            minX >= 0 && minY >= 0)
        {
            return;
        }

        // @see WindowController.as — save and disable accommodate flags
        uint savedParam = param1._param;
        param1._param &= ~(uint)(131072 | 147456);

        // @see WindowController.as — offset children if min coords are negative
        if (minX < 0 || minY < 0)
        {
            float offsetX = minX < 0 ? -minX : 0;
            float offsetY = minY < 0 ? -minY : 0;
            for (int i = 0;
                 i < param1.numChildren;
                 i++)
            {
                IWindow? child = param1.GetChildAt(i);
                child?.Offset(offsetX, offsetY);
            }
            newW += offsetX;
            newH += offsetY;
        }

        param1.SetRectangle(param1.x, param1.y, newW, newH);
        param1._param = savedParam;
    }

    /// @see WindowController.as::scaleToAccommodateChildren (instance)
    /// Scales and repositions window to fit all visible children.
    public void ScaleToAccommodateChildren()
    {
        if (numChildren == 0)
        {
            return;
        }

        float minX = float.MaxValue,
            minY = float.MaxValue;
        float maxRight = float.MinValue,
            maxBottom = float.MinValue;
        bool hasVisible = false;

        for (int i = 0;
             i < numChildren;
             i++)
        {
            IWindow? child = GetChildAt(i);
            if (child is not { visible: true })
            {
                continue;
            }

            hasVisible = true;

            if (child.x < minX)
            {
                minX = child.x;
            }
            if (child.y < minY)
            {
                minY = child.y;
            }

            float childRight = child.x + child.width;
            float childBottom = child.y + child.height;

            if (childRight > maxRight)
            {
                maxRight = childRight;
            }
            if (childBottom > maxBottom)
            {
                maxBottom = childBottom;
            }
        }

        if (!hasVisible)
        {
            return;
        }

        float newW = maxRight - minX;
        float newH = maxBottom - minY;

        // @see WindowController.as — save and disable accommodate flags + child scale flags
        uint savedParam = _param;
        _param &= ~(uint)(131072 | 147456);

        SetRectangle(base.x + minX, base.y + minY, newW, newH);

        // @see WindowController.as — offset children to origin, masking child scale flags during offset
        if (Math.Abs(minX) > float.Epsilon || Math.Abs(minY) > float.Epsilon)
        {
            for (int i = 0;
                 i < numChildren;
                 i++)
            {
                IWindow? child = GetChildAt(i);
                if (child == null)
                    continue;

                // @see WindowController.as — save child's scale/center bits, clear during offset, restore after
                uint childSavedBits = child.param & (192u | 3072u);
                child.SetParamFlag(192u | 3072u, false);
                child.Offset(-minX, -minY);
                child.param = (child.param & ~(192u | 3072u)) | childSavedBits;
            }
        }

        _param = savedParam;
    }

    /// @see WindowController.as::updateScaleRelativeToParent
    /// Adjusts this window's position/size relative to parent based on scaling param flags.
    /// Uses _parentRectOnAdd (AS3 var_2115) to calculate deltas from parent's original size.
    protected void UpdateScaleRelativeToParent()
    {
        if (_parent == null)
        {
            return;
        }

        bool hasHScale = !TestParamFlag(0, 192);
        bool hasVScale = !TestParamFlag(0, 3072);

        float newX = base.x;
        float newY = base.y;
        float newW = base.width;
        float newH = base.height;

        if (hasHScale || hasVScale)
        {
            if (hasHScale)
            {
                // @see WindowController.as — delta from parent's size when child was added
                float deltaW = _parent.width - _parentRectOnAdd.Size.X;
                uint hFlag = _param & 192;

                if (hFlag == 128) // STRETCH
                {
                    newW += deltaW;
                }
                else if (hFlag == 64) // MOVE
                {
                    newX += deltaW;
                }
                else if (hFlag == 192) // CENTER
                {
                    if (_parent.width < newW && TestParamFlag(16))
                    {
                        newX = 0;
                    }
                    else
                    {
                        newX = MathF.Floor(_parent.width / 2f) - MathF.Floor(newW / 2f);
                    }
                }
            }

            if (hasVScale)
            {
                float deltaH = _parent.height - _parentRectOnAdd.Size.Y;
                uint vFlag = _param & 3072;

                switch (vFlag)
                {
                    // STRETCH
                    case 2048:
                        newH += deltaH;
                        break;
                    // MOVE
                    case 1024:
                        newY += deltaH;
                        break;
                    // CENTER
                    case 3072 when _parent.height < newH && TestParamFlag(16):
                        newY = 0;
                        break;
                    case 3072:
                        newY = MathF.Floor(_parent.height / 2f) - MathF.Floor(newH / 2f);
                        break;
                }
            }

            // @see WindowController.as — save and disable reflect flags during setRectangle
            uint savedParam = _param;
            _param &= ~(uint)12582912; // REFLECT_RESIZE_TO_PARENT mask

            SetRectangle(newX, newY, newW, newH);

            _param = savedParam;
        }
        else if (TestParamFlag(32)) // BOUND_TO_PARENT_RECT
        {
            if (_parent == null)
            {
                return;
            }

            if (newX < 0)
            {
                newX = 0;
            }
            if (newY < 0)
            {
                newY = 0;
            }
            if (newX + newW > _parent.width)
            {
                newX -= newX + newW - _parent.width;
            }
            if (newY + newH > _parent.height)
            {
                newY -= newY + newH - _parent.height;
            }
            if (newX + newW > _parent.width)
            {
                newW -= newX + newW - _parent.width;
            }
            if (newY + newH > _parent.height)
            {
                newH -= newY + newH - _parent.height;
            }

            if (!(Math.Abs(newX - base.x) > float.Epsilon) && !(Math.Abs(newY - base.y) > float.Epsilon) &&
                !(Math.Abs(newW - base.width) > float.Epsilon) && !(Math.Abs(newH - base.height) > float.Epsilon))
            {
                return;
            }

            uint savedParam = _param;
            _param &= ~(uint)12582912;

            SetRectangle(newX, newY, newW, newH);

            _param = savedParam;
        }
    }

    /// @see WindowController.as::notifyChildren — broadcast event to all children
    private void NotifyChildren(WindowEvent param1)
    {
        // @see WindowController.as — AS3 iterates a snapshot (for-each on Array copy)
        IWindow[] snapshot = _children.ToArray();

        foreach (IWindow child in snapshot)
        {
            if (child is WindowController controller)
            {
                controller.Update(this, param1);
            }
        }
    }

    /// @see WindowController.as::setStateFlag (lines 1704-1713)
    public override void SetStateFlag(uint param1, bool param2 = true)
    {
        uint oldState = _state;

        base.SetStateFlag(param1, param2);

        if (_state != oldState)
        {
            // @see WindowController.as line 1710 — renderDynamicStyle before invalidate
            RenderDynamicStyle();
            _context?.Invalidate(this, null, Class3655.STATE);
        }
    }

    /// @see WindowController.as::setParamFlag — override: if flag 16 changes, reconfigure GC
    /// AS3: when flag 16 is cleared and no GC → create GC + invalidate
    /// AS3: when flag 16 is set and has GC → just invalidate (DON'T dispose)
    public override void SetParamFlag(uint param1, bool param2 = true)
    {
        uint oldParam = _param;

        base.SetParamFlag(param1, param2);

        if ((oldParam ^ _param) == 0 || (param1 & 16) == 0)
        {
            return;
        }

        if (!TestParamFlag(16) && _var1650 == null)
        {
            _var1650 = GetGraphicContext(true);
        }

        _context?.Invalidate(this, null, Class3655.REDRAW);
    }

    /// @see WindowController.as::set etching (line 486-488) — base is no-op, overridden by BitmapDataController
    public virtual void SetEtching(uint color, float px, float py) { }

    /// @see WindowController.as::renderDynamicStyle (lines 1260-1291)
    internal void RenderDynamicStyle()
    {
        if (string.IsNullOrEmpty(_dynamicStyle))
        {
            return;
        }

        if (_cachedDynamicStyle == null || _cachedDynamicStyle.Name != _dynamicStyle)
        {
            _cachedDynamicStyle = Class3622.GetStyle(_dynamicStyle);
        }

        // @see WindowController.as — state priority: disabled(32) > pressed(16) > hover(4) > default(0)
        uint stateForStyle;

        if (GetStateFlag(32))
        {
            stateForStyle = 32;
        }
        else if (GetStateFlag(16))
        {
            stateForStyle = 16;
        }
        else if (GetStateFlag(4))
        {
            stateForStyle = 4;
        }
        else
        {
            stateForStyle = 0;
        }

        ApplyDynamicStyleByState(this, _cachedDynamicStyle, stateForStyle);

        if (_children.Count > 0)
        {
            RecursivelyUpdateChildrensDynamicStyles(_children, stateForStyle);
        }
    }

    /// @see WindowController.as::applyDynamicStyleByState (lines 1293-1318)
    private static void ApplyDynamicStyleByState(WindowController param1, DynamicStyle param2, uint param3)
    {
        Dictionary<string, object?> style = param2.GetStyleByWindowState(param3);

        // @see WindowController.as — apply offsets
        param1._offsetX = style.TryGetValue("offsetX", out object? ox) && ox is double dox ? (float)dox : 0;
        param1._offsetY = style.TryGetValue("offsetY", out object? oy) && oy is double doy ? (float)doy : 0;

        // @see WindowController.as line 1299-1307 — apply colorTransform to GC or store as pending
        ColorTransform ct = BuildColorTransformFromStyle(style);
        if (param1.HasGraphicsContext() && param1._var1650 != null)
        {
            // Godot adaptation: apply color modulate to the GC display node
            // Convert ColorTransform mult/offset to a single Godot Color for modulate
            param1._var1650.Modulate = new Color(
                ct.RedMultiplier, ct.GreenMultiplier, ct.BlueMultiplier, ct.AlphaMultiplier
            );
        }
        else
        {
            param1._dynamicStyleColor = ct;
            param1._context?.Invalidate(param1, null, Class3655.REDRAW);
        }

        // @see WindowController.as — apply etching
        if (style.TryGetValue("etchingPoint", out object? epObj) && epObj is double[] { Length: >= 2 } ep)
        {
            uint etchColor = style.TryGetValue("etchingColor", out object? ecObj) && ecObj is uint ec ? ec : 0u;
            param1.SetEtching(etchColor, (float)ep[0], (float)ep[1]);
            param1._context?.Invalidate(param1, null, Class3655.REDRAW);
        }
        else
        {
            // @see WindowController.as line 1316 — clear etching: [0, 0, 1]
            param1.SetEtching(0, 0, 1);
            param1._context?.Invalidate(param1, null, Class3655.REDRAW);
        }
    }

    /// Build a ColorTransform from the "colorTransform" array in a style dictionary.
    /// Returns identity ColorTransform if not present.
    private static ColorTransform BuildColorTransformFromStyle(Dictionary<string, object?> style)
    {
        if (!style.TryGetValue("colorTransform", out object? ctObj) || ctObj is not double[] ct || ct.Length < 8)
        {
            return new ColorTransform();
        }

        return new ColorTransform(
            (float)ct[0], (float)ct[1], (float)ct[2], (float)ct[3],
            (float)ct[4], (float)ct[5], (float)ct[6], (float)ct[7]
        );
    }

    /// @see WindowController.as::recursivelyUpdateChildrensDynamicStyles (lines 1321-1333)
    private void RecursivelyUpdateChildrensDynamicStyles(List<IWindow> param1, uint param2)
    {
        foreach (IWindow child in param1)
        {
            if (child is not WindowController childCtrl)
            {
                continue;
            }

            DynamicStyle? childStyle = _cachedDynamicStyle?.GetChildStyle(childCtrl);
            if (childStyle != null)
            {
                ApplyDynamicStyleByState(childCtrl, childStyle, param2);
            }

            if (childCtrl._children.Count > 0)
            {
                childCtrl.RecursivelyUpdateChildrensDynamicStyles(childCtrl._children, param2);
            }
        }
    }

    /// @see WindowController.as::set properties — base virtual for property application
    public virtual void ApplyProperties(PropertyStruct[] properties) { }

    /// @see WindowController.as::get properties — returns current property values as PropertyStruct array
    public virtual PropertyStruct[] GetProperties()
    {
        return [];
    }

    /// @see WindowController.as::getGlobalPosition — optimized path using _parent chain
    public override Vector2 GetGlobalPosition()
    {
        float gx = base.x;
        float gy = base.y;
        WindowController? current = _parent;

        while (current != null)
        {
            gx += current.x;
            gy += current.y;
            current = current._parent;
        }

        return new Vector2(gx, gy);
    }

    public override string ToString()
    {
        return
            $"WindowController {{ name: {name}, id: {id}, instance: {_instanceId}, rect: ({base.x},{base.y},{base.width},{base.height}) }}";
    }

    /// @see WindowController.as::notifyEventListeners
    /// AS3 order: procedure FIRST, then dispatchEvent (if not prevented)
    protected internal void NotifyEventListeners(WindowEvent evt)
    {
        _procedure?.Invoke(evt, this);

        if (!evt.IsWindowOperationPrevented())
        {
            _eventDispatcher?.DispatchEvent(evt);
        }
    }

    private WindowEvent? DispatchCancelable(string type)
    {
        WindowEvent evt = new(type, this, null, true);

        NotifyEventListeners(evt);

        return evt;
    }

    /// @see WindowController.as — in AS3, all events flow through update() which handles
    /// procedure+dispatch in its preamble, then does internal work.
    private void DispatchSimple(string type)
    {
        WindowEvent evt = new(type, this, null, false);
        Update(this, evt);
    }

    private void DispatchChildEvent(string type, IWindow child)
    {
        WindowEvent evt = new(type, this, child, false);
        Update(this, evt);
    }

    /// @see WindowController.as::setupGraphicsContext — wire child GC into parent GC tree
    /// Only wires GC if parent's GC has been set up OR child already has a GC.
    private void SetupChildGraphicsContext(IWindow param1)
    {
        if (this is not IGraphicContextHost parentHost)
        {
            return;
        }

        if (!_graphicsContextSetUp && param1 is IGraphicContextHost childHostCheck && childHostCheck.GetGraphicContext(false) == null)
        {
            return;
        }

        IGraphicContext? parentGc = parentHost.GetGraphicContext(false);

        if (parentGc != null && param1 is IGraphicContextHost childHost)
        {
            IGraphicContext? childGc = childHost.GetGraphicContext(true);

            if (childGc != null)
            {
                parentGc.AddChildContext(childGc);
            }
        }
    }

    /// @see WindowController.as — detach child GC from parent GC tree on removal
    private static void TeardownChildGraphicsContext(IWindow param1)
    {
        if (param1 is not IGraphicContextHost childHost)
        {
            return;
        }

        IGraphicContext? childGc = childHost.GetGraphicContext(false);

        if (childGc?.DisplayNode?.GetParent() != null)
        {
            childGc.DisplayNode.GetParent().RemoveChild(childGc.DisplayNode);
        }
    }

    private static void SetChildParent(IWindow child, IWindow? newParent)
    {
        switch (child)
        {
            case WindowController controller:
                controller._parent = newParent as WindowController;
                // Also set the WindowModel._parent so IWindow.parent returns correctly.
                controller.SetParentInternal(newParent);
                // @see WindowController.as::var_2115 — store parent's rect when child is added
                if (newParent != null)
                {
                    controller._parentRectOnAdd = new Rect2(newParent.x, newParent.y, newParent.width, newParent.height);
                }
                else
                {
                    controller._parentRectOnAdd = default;
                }
                break;
            case WindowModel model:
                model.SetParentInternal(newParent);
                break;
        }
    }

    private static void DispatchToChild(IWindow child, string type)
    {
        if (child is not WindowController controller)
        {
            return;
        }

        WindowEvent evt = new(type, child, null, false);
        controller.Update(controller, evt);
    }
}
