// @see core/window/components/TextLinkController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Theme;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/TextLinkController.as
/// Text controller with interactive tooltip/cursor support.
public class TextLinkController : TextController, ITextLinkWindow, IInteractiveWindow
{
    private string _mouseCursorType = "";
    private string _toolTipCaption = "";
    private readonly Dictionary<uint, uint> _cursorByState = new();

    /// @see TextLinkController.as::TextLinkController (default)
    public TextLinkController() : base() { }

    /// @see TextLinkController.as::TextLinkController (name + rect)
    public TextLinkController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see TextLinkController.as::TextLinkController (full AS3 11-param signature)
    public TextLinkController
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
    ) : base(param1, param2, param3, (uint)((param4 | 1) & ~16u), param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see TextLinkController.as — read theme defaults
        try
        {
            IWindowFactory factory = param5.GetWindowFactory();

            {
                IThemeManager? themeManager = factory.GetThemeManager();
                IPropertyMap? propDefaults = themeManager?.GetPropertyDefaults(param3);

                if (propDefaults?.GetValue("toolTipDelay")?.value is uint delay)
                {
                    ToolTipDelay = delay;
                }

                if (propDefaults?.GetValue("toolTipCaption")?.value is string tipCaption)
                {
                    _toolTipCaption = tipCaption;
                }

                if (propDefaults?.GetValue("toolTipIsDynamic")?.value is bool isDynamic)
                {
                    ToolTipIsDynamic = isDynamic;
                }

                if (propDefaults?.GetValue("interactiveCursorDisabled")?.value is bool cursorDisabled)
                {
                    InteractiveCursorDisabled = cursorDisabled;
                }
            }
        }
        catch
        {
            // Fallback: keep defaults
        }

        // @see TextLinkController.as — enable immediate click and zero mouse threshold
        _mouseThreshold = 0;
    }

    /// @see TextLinkController.as::get mouseCursorType
    public string MouseCursorType
    {
        get => _mouseCursorType;
        set => _mouseCursorType = value ?? "";
    }

    /// @see TextLinkController.as::get toolTipCaption
    public string ToolTipCaption
    {
        get => _toolTipCaption;
        set => _toolTipCaption = value ?? "";
    }

    /// @see TextLinkController.as::get toolTipDelay
    public uint ToolTipDelay { get; set; } = 500;

    /// @see TextLinkController.as::get toolTipIsDynamic
    public bool ToolTipIsDynamic { get; set; }

    /// @see TextLinkController.as::get interactiveCursorDisabled
    public bool InteractiveCursorDisabled { get; set; }

    /// @see TextLinkController.as::setMouseCursorForState
    public uint SetMouseCursorForState(uint state, uint cursor)
    {
        uint previous = _cursorByState.TryGetValue(state, out uint old) ? old : 0;

        if (cursor is 0 or uint.MaxValue)
        {
            _cursorByState.Remove(state);
        }
        else
        {
            _cursorByState[state] = cursor;
        }

        return previous;
    }

    /// @see TextLinkController.as::getMouseCursorByState
    public uint GetMouseCursorByState(uint state)
    {
        return _cursorByState.GetValueOrDefault(state, 0u);
    }

    /// @see TextLinkController.as::showToolTip / hideToolTip
    public virtual void ShowToolTip()
    {
        /* Deferred: tooltip system */
    }
    public virtual void HideToolTip()
    {
        /* Deferred: tooltip system */
    }

    /// @see TextLinkController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        // @see TextLinkController.as — process interactive events
        return base.Update(param1, param2);
    }

    /// @see TextLinkController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        _cursorByState.Clear();

        return base.Destroy();
    }

    // IInteractiveWindow explicit property implementations (public members have matching names)

    /// @see TextLinkController.as — ToolTipCaption bridges to public property
    string? IInteractiveWindow.ToolTipCaption
    {
        get => ToolTipCaption;
        set => ToolTipCaption = value ?? "";
    }

    /// @see TextLinkController.as — ToolTipDelay bridges to public property
    uint IInteractiveWindow.ToolTipDelay
    {
        get => ToolTipDelay;
        set => ToolTipDelay = value;
    }

    /// @see TextLinkController.as — ToolTipIsDynamic bridges to public property
    bool IInteractiveWindow.ToolTipIsDynamic
    {
        get => ToolTipIsDynamic;
        set => ToolTipIsDynamic = value;
    }

    /// @see TextLinkController.as — InteractiveCursorDisabled bridges to public property
    bool IInteractiveWindow.InteractiveCursorDisabled
    {
        get => InteractiveCursorDisabled;
        set => InteractiveCursorDisabled = value;
    }
}
