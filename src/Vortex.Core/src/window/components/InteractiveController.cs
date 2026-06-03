// @see core/window/components/InteractiveController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Services;
using Vortex.Core.Window.Theme;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// <summary>
/// Base controller for interactive windows. Provides tooltip management,
/// custom cursor per state, and IInteractiveWindow implementation.
/// </summary>
/// @see core/window/components/InteractiveController.as
public class InteractiveController : WindowController, IInteractiveWindow
{
    protected uint _toolTipDelay = 500;
    protected string? _toolTipCaption;
    protected bool _toolTipIsDynamic;
    protected bool _cursorDisabled;
    private Dictionary<uint, uint>? _cursorByState;

    /// @see InteractiveController.as::InteractiveController (default)
    public InteractiveController() : base() { }

    /// @see InteractiveController.as::InteractiveController (name + rect)
    public InteractiveController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see InteractiveController.as::InteractiveController (full AS3 11-param signature)
    public InteractiveController
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
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, param4 | 1, param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see InteractiveController.as — AS3 reads 4 theme property defaults in constructor
        try
        {
            IThemeManager? themeManager = param5.GetWindowFactory()?.GetThemeManager();
            IPropertyMap? propDefaults = themeManager?.GetPropertyDefaults(param3);

            if (propDefaults != null)
            {
                PropertyStruct? delayProp = propDefaults.GetValue("tool_tip_delay");
                if (delayProp?.value != null)
                {
                    _toolTipDelay = Convert.ToUInt32(delayProp.value);
                }

                PropertyStruct? captionProp = propDefaults.GetValue("tool_tip_caption");
                if (captionProp?.value is string cap)
                {
                    _toolTipCaption = cap;
                }

                PropertyStruct? dynProp = propDefaults.GetValue("tool_tip_is_dynamic");
                if (dynProp?.value is bool dyn)
                {
                    _toolTipIsDynamic = dyn;
                }

                PropertyStruct? cursorProp = propDefaults.GetValue("interactive_cursor_disabled");
                if (cursorProp?.value is bool dis)
                {
                    _cursorDisabled = dis;
                }
            }
        }
        catch
        {
            // Fallback: keep field defaults
        }
    }

    /// <summary>
    /// Process interactive window events: tooltip begin/update/end on hover.
    /// </summary>
    /// @see InteractiveController.as::processInteractiveWindowEvents
    public static void ProcessInteractiveWindowEvents(IInteractiveWindow window, WindowEvent evt)
    {
        bool isDynamic = window.ToolTipIsDynamic;
        string? caption = window.ToolTipCaption;

        if (isDynamic)
        {
            WindowToolTipAgent? agent = GetToolTipAgent(window);

            if (agent == null)
            {
                return;
            }

            switch (evt.type)
            {
                case WindowMouseEvent.OVER:
                    agent.Begin((IWindow)window);
                    break;
                case WindowMouseEvent.MOVE:
                    agent.UpdateCaption((IWindow)window);
                    break;
                case WindowMouseEvent.OUT:
                    agent.End((IWindow)window);
                    break;
            }
        }
        else if (!string.IsNullOrEmpty(caption))
        {
            WindowToolTipAgent? agent = GetToolTipAgent(window);

            if (agent == null)
            {
                return;
            }

            switch (evt.type)
            {
                case WindowMouseEvent.OVER:
                    agent.Begin((IWindow)window);
                    break;
                case WindowMouseEvent.OUT:
                    agent.End((IWindow)window);
                    break;
            }
        }
    }

    private static WindowToolTipAgent? GetToolTipAgent(IInteractiveWindow window)
    {
        if (window is WindowController
            {
                context: WindowContext ctx,
            })
        {
            return (ctx.GetWindowServices() as ServiceManager)?.ToolTipAgent;
        }

        return null;
    }

    // IInteractiveWindow property implementations

    /// @see InteractiveController.as::get/set toolTipCaption
    string? IInteractiveWindow.ToolTipCaption
    {
        get => _toolTipCaption;
        set => _toolTipCaption = value;
    }

    /// @see InteractiveController.as::get/set toolTipDelay
    uint IInteractiveWindow.ToolTipDelay
    {
        get => _toolTipDelay;
        set => _toolTipDelay = value;
    }

    /// @see InteractiveController.as::get/set toolTipIsDynamic
    bool IInteractiveWindow.ToolTipIsDynamic
    {
        get => _toolTipIsDynamic;
        set => _toolTipIsDynamic = value;
    }

    /// @see InteractiveController.as::get/set interactiveCursorDisabled
    bool IInteractiveWindow.InteractiveCursorDisabled
    {
        get => _cursorDisabled;
        set => _cursorDisabled = value;
    }

    /// @see InteractiveController.as::showToolTip
    void IInteractiveWindow.ShowToolTip() { }

    /// @see InteractiveController.as::hideToolTip
    void IInteractiveWindow.HideToolTip() { }

    /// @see InteractiveController.as::setMouseCursorForState — returns previous cursor for that state
    public uint SetMouseCursorForState(uint stateFlags, uint cursorType)
    {
        if (TestStateFlag(32))
        {
            return 0;
        }

        _cursorByState ??= new Dictionary<uint, uint>();

        uint previous = _cursorByState.TryGetValue(stateFlags, out uint old) ? old : 0;

        if (cursorType is 0 or uint.MaxValue)
        {
            _cursorByState.Remove(stateFlags);
        }
        else
        {
            _cursorByState[stateFlags] = cursorType;
        }

        return previous;
    }

    /// @see InteractiveController.as::getMouseCursorByState
    public uint GetMouseCursorByState(uint stateFlags)
    {
        if (_cursorByState == null)
        {
            return 0;
        }

        return _cursorByState.TryGetValue(stateFlags, out uint cursor) ? cursor : 0;
    }

    /// @see InteractiveController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (param1 == this)
        {
            ProcessInteractiveWindowEvents(this, param2);
        }

        return base.Update(param1, param2);
    }

    /// @see InteractiveController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "tool_tip_caption":
                    _toolTipCaption = prop.value?.ToString();
                    break;
                case "tool_tip_delay":
                    if (prop.value is uint d)
                    {
                        _toolTipDelay = d;
                    }
                    break;
                case "tool_tip_is_dynamic":
                    if (prop.value is bool dyn)
                    {
                        _toolTipIsDynamic = dyn;
                    }
                    break;
                case "interactive_cursor_disabled":
                    if (prop.value is bool dis)
                    {
                        _cursorDisabled = dis;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }
}
