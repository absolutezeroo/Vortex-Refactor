// @see core/window/utils/MouseEventProcessor.as

using System;
using System.Linq;

using Godot;

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Enum;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;

using WinController = Vortex.Core.Window.WindowController;

namespace Vortex.Core.Window.Utils;

/// <summary>
/// Core mouse event processing. Hit-tests windows under the cursor, dispatches
/// OVER/OUT/CLICK/DOWN/UP events, tracks hover/focus state, and manages cursor.
/// </summary>
/// @see core/window/utils/MouseEventProcessor.as
public class MouseEventProcessor : IEventProcessor, IDisposable
{
    /// <summary>
    /// Default cursor type per window state index.
    /// Indices: 0=active, 1=focused, 2=hovering, 3=locked, 4=selected, 5=pressed, 6=disabled.
    /// </summary>
    protected static uint[]? _cursorByStateIndex;

    /// <summary>
    /// State bit masks corresponding to each cursor index.
    /// </summary>
    protected static uint[]? _stateMasks;

    private Vector2 _lastMousePos;
    private WinController? _currentHovered;
    private WinController? _lastClickTarget;
    private WinController? _lastMouseDownTarget;
    private WinController? _focusedControl;
    private WindowRenderer? _renderer;
    private IDesktopWindow? _desktop;
    private List<IInputEventTracker>? _eventTrackers;

    /// @see MouseEventProcessor.as::MouseEventProcessor
    public MouseEventProcessor()
    {
        _lastMousePos = new Vector2(-1, -1);
        _cursorByStateIndex ??= new uint[]
            {
                Class3549.ARROW_LINK, // active
                Class3549.DEFAULT,    // focused
                Class3549.ARROW_LINK, // hovering
                Class3549.ARROW_LINK, // locked
                Class3549.ARROW_LINK, // selected
                Class3549.DEFAULT,    // pressed
                Class3549.ARROW_LINK, // disabled
            };
        _stateMasks ??= new uint[]
            {
                Class3466.WINDOW_STATE_ACTIVE,   // 1
                Class3466.WINDOW_STATE_FOCUSED,  // 2
                Class3466.WINDOW_STATE_HOVERING, // 4
                Class3466.WINDOW_STATE_LOCKED,   // 64
                Class3466.WINDOW_STATE_SELECTED, // 8
                Class3466.WINDOW_STATE_PRESSED,  // 16
                Class3466.WINDOW_STATE_DISABLED, // 32
            };
    }

    /// @see MouseEventProcessor.as::setMouseCursorByState
    public static void SetMouseCursorByState(uint state, uint cursor)
    {
        if (_stateMasks == null || _cursorByStateIndex == null)
        {
            return;
        }
        int idx = Array.IndexOf(_stateMasks, state);
        if (idx > -1)
        {
            _cursorByStateIndex[idx] = cursor;
        }
    }

    /// @see MouseEventProcessor.as::getMouseCursorByState
    public static uint GetMouseCursorByState(uint state)
    {
        if (_stateMasks == null || _cursorByStateIndex == null)
        {
            return 0;
        }
        int i = _stateMasks.Length;
        while (i-- > 0)
        {
            if ((state & _stateMasks[i]) > 0)
            {
                return _cursorByStateIndex[i];
            }
        }
        return 0;
    }

    /// <summary>
    /// Convert a Godot InputEvent to a WindowMouseEvent for a target window.
    /// Godot adaptation: maps InputEventMouseButton/InputEventMouseMotion to WME_ constants.
    /// </summary>
    /// @see MouseEventProcessor.as::convertMouseEventType
    protected static WindowMouseEvent ConvertMouseEventType
    (
        InputEvent godotEvent, IWindow target, IWindow? related, string? overrideType = null
    )
    {
        float stageX = 0,
            stageY = 0;
        bool altKey = false,
            ctrlKey = false,
            shiftKey = false,
            buttonDown = false;
        int delta = 0;

        if (godotEvent is InputEventMouse mouseEvt)
        {
            stageX = mouseEvt.Position.X;
            stageY = mouseEvt.Position.Y;
        }
        if (godotEvent is InputEventWithModifiers modEvt)
        {
            altKey = modEvt.AltPressed;
            ctrlKey = modEvt.CtrlPressed;
            shiftKey = modEvt.ShiftPressed;
        }
        if (godotEvent is InputEventMouseButton btnEvt)
        {
            buttonDown = btnEvt.Pressed;
            if (btnEvt.ButtonIndex == MouseButton.WheelUp)
            {
                delta = 1;
            }
            else if (btnEvt.ButtonIndex == MouseButton.WheelDown)
            {
                delta = -1;
            }
        }

        Vector2 localPos = Vector2.Zero;
        if (target is WinController wc)
        {
            localPos = wc.ConvertPointFromGlobalToLocalSpace(new Vector2(stageX, stageY));
        }

        string eventType = overrideType ?? GetGodotEventType(godotEvent);
        string wmeType = eventType switch
        {
            "mouseMove" => WindowMouseEvent.MOVE,
            "mouseOver" => WindowMouseEvent.OVER,
            "mouseOut" => WindowMouseEvent.OUT,
            "rollOut" => WindowMouseEvent.ROLL_OUT,
            "rollOver" => WindowMouseEvent.ROLL_OVER,
            "click" => WindowMouseEvent.CLICK,
            "doubleClick" => WindowMouseEvent.DOUBLE_CLICK,
            "mouseDown" => WindowMouseEvent.DOWN,
            "mouseUp" => IsInsideBounds(localPos, target)
                ? WindowMouseEvent.UP
                : WindowMouseEvent.UP_OUTSIDE,
            "mouseWheel" => WindowMouseEvent.WHEEL,
            _ => "",
        };

        return WindowMouseEvent.Allocate(
            wmeType, target, related,
            localPos.X, localPos.Y, stageX, stageY,
            altKey, ctrlKey, shiftKey, buttonDown, delta
        );
    }

    /// <summary>
    /// Classify a Godot InputEvent into a Flash-equivalent event type string.
    /// </summary>
    private static string GetGodotEventType(InputEvent evt)
    {
        if (evt is InputEventMouseMotion)
        {
            return "mouseMove";
        }

        if (evt is InputEventMouseButton btnEvt)
        {
            if (btnEvt.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
            {
                return "mouseWheel";
            }

            if (btnEvt.ButtonIndex == MouseButton.Left)
            {
                if (btnEvt.DoubleClick)
                {
                    return "doubleClick";
                }
                if (btnEvt.Pressed)
                {
                    return "mouseDown";
                }
                return "mouseUp";
            }
        }

        return "";
    }

    private static bool IsInsideBounds(Vector2 localPos, IWindow window)
    {
        return localPos is { X: > -1, Y: > -1 } &&
               localPos.X < window.width && localPos.Y < window.height;
    }

    public bool Disposed { get; private set; }

    /// @see MouseEventProcessor.as::dispose
    public void Dispose()
    {
        if (!Disposed)
        {
            Disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    /// @see IEventProcessor — typed implementation
    void IEventProcessor.Process(EventProcessorState state, IEventQueue queue)
    {
        ProcessEvents(state, queue);
    }

    /// @see MouseEventProcessor.as::process
    public void ProcessEvents(EventProcessorState state, IEventQueue queue)
    {
        if (queue is GenericEventQueue { EventCount: 0 })
        {
            return;
        }

        _desktop = state.Desktop;
        _currentHovered = state.CurrentHoveredWindow as WinController;
        _lastClickTarget = state.LastClickTarget as WinController;
        _lastMouseDownTarget = state.LastMouseDownTarget as WinController;
        _focusedControl = state.FocusedControl as WinController;
        _renderer = state.Renderer;
        _eventTrackers = state.EventTrackers;

        queue.Begin();
        _lastMousePos = new Vector2(-1, -1);

        uint cursorType = 0;
        List<IWindow>? childrenAtPoint = null;
        object? rawEvent;

        while ((rawEvent = queue.Next()) != null)
        {
            if (rawEvent is not InputEvent godotEvent)
            {
                continue;
            }

            float stageX = 0,
                stageY = 0;
            if (godotEvent is InputEventMouse mouseEvt)
            {
                stageX = mouseEvt.Position.X;
                stageY = mouseEvt.Position.Y;
            }

            if (stageX != _lastMousePos.X || stageY != _lastMousePos.Y)
            {
                _lastMousePos = new Vector2(stageX, stageY);
                childrenAtPoint = new List<IWindow>();
                _desktop?.GroupParameterFilteredChildrenUnderPoint(_lastMousePos, childrenAtPoint, 1);
            }

            int count = childrenAtPoint?.Count ?? 0;
            string flashType = GetGodotEventType(godotEvent);

            if (count == 0)
            {
                WinController? desktopCtrl = _desktop as WinController;

                switch (flashType)
                {
                    case "mouseMove" when
                        _currentHovered != null && _currentHovered != desktopCtrl &&
                        !_currentHovered.disposed:
                        {
                            Vector2 globalPos = _currentHovered.GetGlobalPosition();
                            WindowMouseEvent outEvt = WindowMouseEvent.Allocate(
                                WindowMouseEvent.OUT, _currentHovered, null,
                                stageX - globalPos.X, stageY - globalPos.Y,
                                stageX, stageY
                            );
                            _currentHovered.Update(_currentHovered, outEvt);
                            _currentHovered = desktopCtrl;
                            break;
                        }
                    case "mouseDown":
                        {
                            IWindow? active = _desktop?.GetActiveWindow();
                            active?.Deactivate();
                            break;
                        }
                }
            }

            // For mouseUp, ensure lastMouseDownTarget is in the list
            if (flashType == "mouseUp" && _lastMouseDownTarget != null)
            {
                if (childrenAtPoint != null && !childrenAtPoint.Contains(_lastMouseDownTarget))
                {
                    childrenAtPoint.Add(_lastMouseDownTarget);
                    count++;
                }
                else if (childrenAtPoint == null)
                {
                    childrenAtPoint = new List<IWindow>
                    {
                        _lastMouseDownTarget,
                    };
                    count = 1;
                }
            }

            // Process children from back to front (highest z-order first)
            for (int i = count - 1;
                 i >= 0;
                 i--)
            {
                if (childrenAtPoint![i] is not WinController child)
                {
                    continue;
                }

                WinController? hitTarget = PassMouseEvent(child, godotEvent);

                if (hitTarget is not { visible: true })
                {
                    continue;
                }

                switch (flashType)
                {
                    // Handle OVER/OUT transitions
                    case "mouseMove" when hitTarget != _currentHovered:
                        {
                            if (_currentHovered is { disposed: false })
                            {
                                Vector2 globalPos = _currentHovered.GetGlobalPosition();
                                WindowMouseEvent outEvt = WindowMouseEvent.Allocate(
                                    WindowMouseEvent.OUT, _currentHovered, hitTarget,
                                    stageX - globalPos.X, stageY - globalPos.Y,
                                    stageX, stageY
                                );
                                _currentHovered.Update(_currentHovered, outEvt);
                            }

                            if (!hitTarget.disposed)
                            {
                                Vector2 globalPos = hitTarget.GetGlobalPosition();
                                WindowMouseEvent overEvt = WindowMouseEvent.Allocate(
                                    WindowMouseEvent.OVER, hitTarget, null,
                                    stageX - globalPos.X, stageY - globalPos.Y,
                                    stageX, stageY
                                );
                                hitTarget.Update(hitTarget, overEvt);
                            }

                            if (!hitTarget.disposed)
                            {
                                _currentHovered = hitTarget;
                            }
                            break;
                        }
                    // Handle CLICK_AWAY on mouseDown
                    case "mouseDown":
                        {
                            if (_focusedControl is { disposed: false } && hitTarget != _focusedControl)
                            {
                                WindowMouseEvent clickAwayEvt = WindowMouseEvent.Allocate(
                                    "WME_CLICK_AWAY", _focusedControl, hitTarget,
                                    float.NaN, float.NaN, stageX, stageY
                                );
                                _focusedControl.Update(_focusedControl, clickAwayEvt);
                            }
                            _focusedControl = hitTarget;
                            break;
                        }
                }

                // Walk parents for IInputProcessorRoot
                IWindow? parentWin = hitTarget.parent;
                while (parentWin is { disposed: false })
                {
                    if (parentWin is IInputProcessorRoot root)
                    {
                        WindowMouseEvent rootEvt = ConvertMouseEventType(godotEvent, parentWin, hitTarget);
                        root.Process(rootEvt);
                        break;
                    }
                    parentWin = parentWin.parent;
                }

                // Cursor selection from interactive window
                if (_currentHovered is IInteractiveWindow interactive)
                {
                    try
                    {
                        if (interactive.InteractiveCursorDisabled)
                        {
                            cursorType = 0;
                        }
                        else
                        {
                            cursorType = interactive.GetMouseCursorByState(_currentHovered.state);
                            if (cursorType == 0)
                            {
                                cursorType = GetMouseCursorByState(_currentHovered.state);
                            }
                        }
                    }
                    catch
                    {
                        cursorType = 0;
                    }
                }

                if (hitTarget != (_desktop as WinController))
                {
                    queue.Remove();
                }
                break;
            }
        }

        queue.End();
        MouseCursorControl.type = cursorType;

        // Write state back
        state.Desktop = _desktop;
        state.CurrentHoveredWindow = _currentHovered;
        state.LastClickTarget = _lastClickTarget;
        state.LastMouseDownTarget = _lastMouseDownTarget;
        state.FocusedControl = _focusedControl;
        state.Renderer = _renderer;
        state.EventTrackers = _eventTrackers;
    }

    /// @see MouseEventProcessor.as::passMouseEvent
    private WinController? PassMouseEvent
    (
        WinController target, InputEvent godotEvent, bool isRerouted = false
    )
    {
        if (target.disposed)
        {
            return null;
        }

        string flashType = GetGodotEventType(godotEvent);

        // Disabled region controllers still receive mouseMove for hover tracking
        if (target.TestStateFlag(Class3466.WINDOW_STATE_DISABLED) &&
            flashType == "mouseMove" &&
            target is RegionController)
        {
            return target;
        }

        if (target.TestStateFlag(Class3466.WINDOW_STATE_DISABLED))
        {
            return null;
        }

        float stageX = 0,
            stageY = 0;

        if (godotEvent is InputEventMouse mouseEvt)
        {
            stageX = mouseEvt.Position.X;
            stageY = mouseEvt.Position.Y;
        }

        Vector2 localPos = target.ConvertPointFromGlobalToLocalSpace(new Vector2(stageX, stageY));

        // mouseUp special handling
        if (flashType == "mouseUp")
        {
            if (_lastMouseDownTarget == null)
            {
                _lastClickTarget = null;
                return null;
            }

            if (target != _lastMouseDownTarget)
            {
                if (_lastMouseDownTarget is { disposed: false })
                {
                    WindowMouseEvent upEvt = ConvertMouseEventType(godotEvent, _lastMouseDownTarget, target);
                    _lastMouseDownTarget.Update(_lastMouseDownTarget, upEvt);
                    _lastClickTarget = null;

                    if (target.disposed)
                    {
                        return null;
                    }
                }
            }
            else
            {
                // hitTest still evaluates for UP vs UP_OUTSIDE in convertMouseEventType
                target.HitTestLocalPoint(localPos);
            }

            _lastMouseDownTarget = null;
        }

        // Ignore mouse events check
        if (target.mouseThreshold == 0)
        {
            // mouseThreshold of 0 = ignore mouse (AS3 _ignoreMouseEvents maps here)
            return null;
        }

        // Hit-test via draw buffer
        Image? drawBuffer = _renderer?.GetDrawBufferForRenderable(target);

        if (!target.HitTestLocalPoint(localPos))
        {
            return null;
        }

        // Route to parent if ROUTE_INPUT_EVENTS_TO_PARENT flag set
        if (target.TestParamFlag(Class3459.WINDOW_PARAM_ROUTE_INPUT_EVENTS_TO_PARENT))
        {
            if (target.parent is WinController parentCtrl)
            {
                return PassMouseEvent(parentCtrl, godotEvent);
            }
        }

        if (!isRerouted)
        {
            switch (flashType)
            {
                case "mouseDown":
                    _lastClickTarget = target;
                    _lastMouseDownTarget = target;
                    break;
                case "click":
                case "doubleClick":
                    if (_lastClickTarget != target)
                    {
                        _lastClickTarget = null;
                        return null;
                    }
                    _lastClickTarget = null;
                    break;
            }
        }

        // Build event type list (doubleClick fires click first)
        List<string> eventTypes = new();
        if (flashType == "doubleClick")
        {
            eventTypes.Add("click");
        }
        eventTypes.Add(flashType);

        bool handled = false;

        foreach (WindowMouseEvent wmeEvt in eventTypes.Select(evtType => ConvertMouseEventType(godotEvent, target, null, evtType)))
        {
            if (target.Update(target, wmeEvt))
            {
                handled = true;
            }

            if (_eventTrackers == null)
            {
                continue;
            }

            foreach (IInputEventTracker tracker in _eventTrackers)
            {
                tracker.EventReceived(wmeEvt, target);
            }
        }

        // If not handled and not rerouted, propagate to parent
        if (!handled && !isRerouted)
        {
            if (target.parent is WinController parentCtrl)
            {
                return PassMouseEvent(parentCtrl, godotEvent);
            }
        }

        return target;
    }
}
