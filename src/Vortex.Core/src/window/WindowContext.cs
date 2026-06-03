// @see WIN63-202407091256-704579380-Source-main/core/window/WindowContext.as

using System;
using System.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Localization;
using Vortex.Core.Runtime;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Motion;
using Vortex.Core.Window.Services;
using Vortex.Core.Window.Utils;
using Vortex.Core.Window.Utils.Tablet;

using IDisposable = System.IDisposable;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/WindowContext.as
public class WindowContext : IWindowContext, IDisposable, IUpdateReceiver
{
    /// @see WindowContext.as::INPUT_MODE_MOUSE
    public const uint INPUT_MODE_MOUSE = 0;

    /// @see WindowContext.as::INPUT_MODE_TOUCH
    public const uint INPUT_MODE_TOUCH = 1;

    /// @see WindowContext.as::ERROR_UNKNOWN
    public const int ERROR_UNKNOWN = 0;

    /// @see WindowContext.as::ERROR_INVALID_WINDOW
    public const int ERROR_INVALID_WINDOW = 1;

    /// @see WindowContext.as::ERROR_WINDOW_NOT_FOUND
    public const int ERROR_WINDOW_NOT_FOUND = 2;

    /// @see WindowContext.as::ERROR_WINDOW_ALREADY_EXISTS
    public const int ERROR_WINDOW_ALREADY_EXISTS = 3;

    /// @see WindowContext.as::ERROR_UNKNOWN_WINDOW_TYPE
    public const int ERROR_UNKNOWN_WINDOW_TYPE = 4;

    /// @see WindowContext.as::ERROR_DURING_EVENT_HANDLING
    public const int ERROR_DURING_EVENT_HANDLING = 5;

    /// @see WindowContext.as::inputEventQueue
    public static IEventQueue? InputEventQueue;

    /// @see WindowContext.as::inputEventProcessor
    private static IEventProcessor? _inputEventProcessor;

    /// @see WindowContext.as::inputModeFlag
    private static uint _inputModeFlag = INPUT_MODE_MOUSE;

    /// @see WindowContext.as::var_1836
    private static IClass3354? _var1836;

    /// @see WindowContext.as::stage
    private static readonly object? _stage;

    private readonly Rect2 _desktopBounds;

    private readonly string _name;

    /// @see WindowContext.as::_linkEventTrackers
    private readonly List<object?> _linkEventTrackers;

    /// @see WindowContext.as::var_4604
    private readonly EventProcessorState var_4604;

    /// @see WindowContext.as::_lastError
    protected Exception? _lastError;

    /// @see WindowContext.as::_localization
    protected ICoreLocalizationManager? _localization;

    private bool _rendering;

    /// @see WindowContext.as::_rootDisplayObject
    /// Godot adaptation: AS3 DisplayObjectContainer → Node2D
    protected Node2D? _rootDisplayObject;

    /// @see WindowContext.as::_throwErrors
    protected bool _throwErrors = true;

    /// @see WindowContext.as::inputEventTrackers
    public IList<IInputEventTracker> inputEventTrackers;

    /// @see WindowContext.as::var_1750
    protected IDesktopWindow? var_1750;

    private bool var_1896;

    private readonly object? var_209;

    /// @see WindowContext.as::var_2890
    protected IResourceManager? var_2890;

    /// @see WindowContext.as::var_3190
    protected object? var_3190;

    /// @see WindowContext.as::var_3528
    protected WindowParser var_3528;

    /// @see WindowContext.as::var_3572
    protected SubstituteParentController? var_3572;

    /// @see WindowContext.as::var_3613
    protected int var_3613 = -1;

    /// @see WindowContext.as::var_3799
    protected IWindowFactory? var_3799;

    /// @see WindowContext.as::var_3890
    protected IWidgetFactory? var_3890;

    /// @see WindowContext.as::WindowContext
    public WindowContext()
        : this("default", null, null, null, null, null, null, null, null, null)
    {
    }

    /// @see WindowContext.as::WindowContext
    /// Godot adaptation: param8 typed as Node2D? (AS3 DisplayObjectContainer).
    public WindowContext
    (
        string param1,
        IClass3354? param2,
        IWindowFactory? param3,
        IWidgetFactory? param4,
        IResourceManager? param5,
        ICoreLocalizationManager? param6,
        object? param7,
        Node2D? param8,
        Rect2? param9,
        IList<object?>? param10
    )
    {
        _name = param1;
        _var1836 = param2;
        _localization = param6;
        var_209 = param7;
        _rootDisplayObject = param8;
        var_3190 = new ServiceManager(this);
        var_3799 = param3;
        var_3890 = param4;
        var_2890 = param5;
        var_3528 = new WindowParser(this);
        inputEventTrackers = new List<IInputEventTracker>();
        _linkEventTrackers = param10 != null
            ?
            [
                ..param10,
            ]
            : [];

        Classes.Init();

        _desktopBounds = param9 ?? new Rect2(0, 0, 800, 600);
        var_1750 = new DesktopController("_CONTEXT_DESKTOP_" + _name, this, _desktopBounds);
        // @see WindowContext.as — share the same list reference (AS3 passes by reference)
        var_4604 = new EventProcessorState(
            _var1836 as WindowRenderer, var_1750 as IDesktopWindow,
            var_1750, null, null, null, inputEventTrackers as List<IInputEventTracker>
        );

        // @see WindowContext.as — initialize desktop limits from viewport bounds
        if (var_1750 is WindowController dCtrl)
        {
            dCtrl.limits ??= new WindowRectLimits();
            dCtrl.limits.MaxWidth = (int)_desktopBounds.Size.X;
            dCtrl.limits.MaxHeight = (int)_desktopBounds.Size.Y;
        }

        // Godot adaptation: wire desktop's display node into the root display object.
        Node2D? desktopNode = var_1750?.GetDisplayObject();

        if (desktopNode != null && _rootDisplayObject != null)
        {
            _rootDisplayObject.AddChild(desktopNode);
        }

        if (InputEventQueue == null || _inputEventProcessor == null)
        {
            inputMode = INPUT_MODE_MOUSE;
        }

        var_3572 = new SubstituteParentController(this);
    }

    /// @see WindowContext.as::get inputMode
    public static uint inputMode
    {
        get => _inputModeFlag;
        set
        {
            if (InputEventQueue is IDisposable disposableQueue)
            {
                disposableQueue.Dispose();
            }

            InputEventQueue = null;

            if (_inputEventProcessor is IDisposable disposableProcessor)
            {
                disposableProcessor.Dispose();
            }

            _inputEventProcessor = null;

            switch (value)
            {
                case INPUT_MODE_MOUSE:
                    InputEventQueue = new MouseEventQueue();
                    _inputEventProcessor = new MouseEventProcessor();
                    _inputModeFlag = value;
                    break;
                case INPUT_MODE_TOUCH:
                    InputEventQueue = new TabletEventQueue();
                    _inputEventProcessor = new TabletEventProcessor();
                    _inputModeFlag = value;
                    break;
                default:
                    inputMode = INPUT_MODE_MOUSE;
                    throw new Exception("Unknown input mode " + value);
            }
        }
    }

    /// @see WindowContext.as::get disposed
    public bool disposed { get; private set; }

    /// @see WindowContext.as::get linkEventTrackers
    public IList<object?> linkEventTrackers => _linkEventTrackers;

    /// @see WindowContext.as — provides access to the renderer for default attribute lookup.
    public static IClass3354? GetRenderer()
    {
        return _var1836;
    }

    /// @see WindowContext.as::getWindowParser
    public WindowParser GetWindowParser()
    {
        return var_3528;
    }

    /// @see WindowContext.as::getWindowFactory
    public IWindowFactory GetWindowFactory()
    {
        return var_3799!;
    }

    /// @see WindowContext.as::getDesktopWindow
    public IDesktopWindow GetDesktopWindow()
    {
        return var_1750!;
    }

    /// @see WindowContext.as::findWindowByName
    public IWindow? FindWindowByName(string param1)
    {
        return var_1750?.FindChildByName(param1);
    }

    /// @see WindowContext.as::findWindowByTag
    public IWindow? FindWindowByTag(string param1)
    {
        return var_1750?.FindChildByTag(param1);
    }

    /// @see WindowContext.as::groupChildrenWithTag
    public uint GroupChildrenWithTag(string param1, IList<IWindow> param2, int param3 = 0)
    {
        return var_1750?.GroupChildrenWithTag(param1, param2, param3) ?? 0;
    }

    /// @see WindowContext.as::create
    public IWindow? Create
    (
        string param1,
        string param2,
        uint param3,
        uint param4,
        uint param5,
        Rect2 param6,
        Action<WindowEvent, IWindow>? param7,
        IWindow? param8,
        uint param9,
        IList<object>? param10 = null,
        string param11 = "",
        IList<string>? param12 = null
    )
    {
        Type? windowClass = Classes.GetWindowClassByType(param3);

        if (windowClass == null)
        {
            HandleError(ERROR_UNKNOWN_WINDOW_TYPE, new Exception($"Failed to solve implementation for window \"{param1}\"!"));
            return null;
        }

        // @see WindowContext.as::create — flag 16 substitute parent
        if (param8 == null && (param5 & 16) != 0)
        {
            param8 = var_3572;
        }

        IWindow? window;

        try
        {
            window = (IWindow?)Activator.CreateInstance(
                windowClass,
                param1, param3, param4, param5, this, param6,
                param8 ?? var_1750, param7, param10, param12, param9, param11
            );
        }
        catch (Exception ex)
        {
            // Fallback: try simpler constructors for partially ported controllers.
            GD.PrintErr($"[WindowContext.Create] FALLBACK for '{param1}' ({windowClass.Name}): {ex.InnerException?.Message ?? ex.Message}\n{ex.InnerException?.StackTrace ?? ex.StackTrace}");
            try
            {
                window = (IWindow?)Activator.CreateInstance(windowClass, param1, param6);
            }
            catch
            {
                window = (IWindow?)Activator.CreateInstance(windowClass);
            }

            if (window != null)
            {
                window.name = param1;
                window.id = (int)param9;
                window.x = param6.Position.X;
                window.y = param6.Position.Y;
                window.width = param6.Size.X;
                window.height = param6.Size.Y;

                IWindow? parent = param8 ?? var_1750;

                parent?.AddChild(window);
            }
        }

        switch (window)
        {
            case null:
                return null;
            // @see WindowContext.as::create — set dynamicStyle after construction + invalidate
            case WindowController controller when !string.IsNullOrEmpty(param11):
                controller._dynamicStyle = param11;
                controller.RenderDynamicStyle();
                Invalidate(controller, null, Class3655.REDRAW);
                break;
        }

        // @see WindowContext.as::create — set caption if non-empty
        if (!string.IsNullOrEmpty(param2))
        {
            window.caption = param2;
        }

        return window;
    }

    /// @see WindowContext.as::destroy
    /// @see WindowContext.as::destroy — checks state != 0x40000000 (destroy sentinel)
    public bool Destroy(IWindow param1)
    {
        if (ReferenceEquals(param1, var_1750))
        {
            var_1750 = null;
        }

        if (param1.state != 0x40000000)
        {
            param1.Destroy();
        }

        return true;
    }

    /// @see WindowContext.as::addMouseEventTracker
    public void AddMouseEventTracker(IInputEventTracker param1)
    {
        if (!inputEventTrackers.Contains(param1))
        {
            inputEventTrackers.Add(param1);
        }
    }

    /// @see WindowContext.as::removeMouseEventTracker
    public void RemoveMouseEventTracker(IInputEventTracker param1)
    {
        inputEventTrackers.Remove(param1);
    }

    /// @see WindowContext.as::getWidgetFactory
    public IWidgetFactory GetWidgetFactory()
    {
        return var_3890!;
    }

    /// @see WindowContext.as::set inputMode
    public virtual void InputMode(object? value)
    {
        if (value == null)
        {
            return;
        }

        inputMode = Convert.ToUInt32(value);
    }

    /// @see WindowContext.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        // Godot adaptation: remove desktop display node from root before destroying.                                                                                                                                                                                                                 
        if (var_1750 != null)
        {
            Node2D? desktopNode = (var_1750 as IDisplayObjectWrapper)?.GetDisplayObject();

            if (desktopNode != null && _rootDisplayObject != null &&
                desktopNode.GetParent() == _rootDisplayObject)
            {
                _rootDisplayObject.RemoveChild(desktopNode);
            }
        }

        var_1750?.Destroy();
        var_1750 = null;

        var_3572?.Destroy();
        var_3572 = null;

        if (var_3190 is IDisposable disposableServices)
        {
            disposableServices.Dispose();
        }

        var_3190 = null;
        var_3528.Dispose();
        var_3528 = null!;
        _localization = null;
        _rootDisplayObject = null;
        var_3799 = null;
        var_3890 = null;
        var_2890 = null;

        // @see WindowContext.as::dispose — nulls renderer reference
        _var1836 = null;

        inputEventTrackers.Clear();
        _linkEventTrackers.Clear();
    }

    /// @see WindowContext.as::getLastError
    public Exception? GetLastError()
    {
        return _lastError;
    }

    /// @see WindowContext.as::getLastErrorCode
    public int GetLastErrorCode()
    {
        return var_3613;
    }

    /// @see WindowContext.as::handleError
    public void HandleError(int param1, Exception param2)
    {
        _lastError = param2;
        var_3613 = param1;

        if (_throwErrors)
        {
            throw param2;
        }
    }

    /// @see WindowContext.as::flushError
    public void FlushError()
    {
        _lastError = null;
        var_3613 = -1;
    }

    /// @see WindowContext.as::getWindowServices
    public object? GetWindowServices()
    {
        return var_3190;
    }

    /// @see WindowContext.as::getResourceManager
    public IResourceManager? GetResourceManager()
    {
        return var_2890;
    }

    /// @see WindowContext.as::registerLocalizationListener
    public void RegisterLocalizationListener(string param1, IWindow param2)
    {
        if (_localization != null && param2 is ILocalizable localizable)
        {
            _localization.RegisterListener(param1, localizable);
        }
    }

    /// @see WindowContext.as::removeLocalizationListener
    public void RemoveLocalizationListener(string param1, IWindow param2)
    {
        if (_localization != null && param2 is ILocalizable localizable)
        {
            _localization.RemoveListener(param1, localizable);
        }
    }

    /// @see WindowContext.as::invalidate
    public void Invalidate(IWindow param1, Rect2? param2, uint param3)
    {
        if (disposed || _var1836 == null)
        {
            return;
        }

        _var1836.AddToRenderQueue(param1, param2, param3);
    }

    /// @see WindowContext.as::update
    /// Godot adaptation: MotionManager.OnTick() replaces AS3's self-managing Timer.
    /// GestureAgentService.Update() is frame-driven instead of Flash Timer(40).
    public void Update(uint param1)
    {
        var_1896 = true;

        if (_lastError != null)
        {
            Exception? error = _lastError;

            _lastError = null;

            throw error;
        }

        _inputEventProcessor?.Process(var_4604, InputEventQueue);

        // Godot adaptation: drive motion system from frame loop (AS3 uses its own Timer)
        if (MotionManager.IsRunning)
        {
            MotionManager.OnTick();
        }

        // Godot adaptation: drive gesture inertial scrolling from frame loop
        if (var_3190 is ServiceManager sm)
        {
            sm.GestureAgent?.Update(param1 / 1000.0);
        }

        var_1896 = false;
    }

    /// @see WindowContext.as::render
    public void Render(uint param1)
    {
        _ = param1;

        _rendering = true;
        _var1836?.Render();
        _rendering = false;
    }

    /// @see WindowContext.as::stageResizedHandler — skip entirely if either dimension < 10
    public void StageResizedHandler(object? param1)
    {
        _ = param1;

        if (var_1750 == null || var_1750.disposed)
        {
            return;
        }

        // @see WindowContext.as — use live viewport size instead of frozen _desktopBounds
        Viewport? vp = _rootDisplayObject?.GetViewport();
        Vector2 viewSize = vp?.GetVisibleRect().Size ?? _desktopBounds.Size;

        // @see WindowContext.as — skip entirely if either dimension < 10
        if (viewSize.X < 10 || viewSize.Y < 10)
        {
            return;
        }

        // @see WindowContext.as — update limits THEN dimensions (AS3 order)
        if (var_1750 is WindowController limitsCtrl && limitsCtrl.limits != null)
        {
            limitsCtrl.limits.MaxWidth = (int)viewSize.X;
            limitsCtrl.limits.MaxHeight = (int)viewSize.Y;
        }

        var_1750.width = viewSize.X;
        var_1750.height = viewSize.Y;
    }
}
