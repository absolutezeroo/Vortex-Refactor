// @see core/window/services/WindowToolTipAgent.as

using Godot;

using Vortex.Core.Window.Components;
using Vortex.Core.Window.Enum;

namespace Vortex.Core.Window.Services;

/// <summary>
/// Tooltip service that shows a tooltip window after a configurable delay.
/// Extends WindowMouseOperator for mouse tracking.
/// Godot adaptation: uses SceneTreeTimer instead of Flash Timer.
/// </summary>
/// @see core/window/services/WindowToolTipAgent.as
public class WindowToolTipAgent : WindowMouseOperator
{
    protected string? _caption;
    protected IWindow? _tooltipWindow;
    protected SceneTreeTimer? _timer;
    protected Vector2 _toolTipOffset;
    protected Vector2 _pointerOffset;
    protected uint _toolTipDelay;
    private IWindowContext? _windowContext;

    /// @see WindowToolTipAgent.as::WindowToolTipAgent
    public WindowToolTipAgent() : base()
    {
        _pointerOffset = Vector2.Zero;
        _toolTipOffset = new Vector2(20, 20);
        _toolTipDelay = 500;
    }

    /// Sets the window context reference for tooltip creation.
    /// @deprecated Prefer accessing context via _window.context at runtime.
    public void SetWindowContext(IWindowContext context)
    {
        _windowContext = context;
    }

    /// @see WindowToolTipAgent.as::begin
    public override IWindow? Begin(IWindow? window, uint flags = 0)
    {
        if (window is not { disposed: false })
        {
            return base.Begin(window, flags);
        }

        if (window is IInteractiveWindow interactive)
        {
            _caption = interactive.ToolTipCaption;
            _toolTipDelay = interactive.ToolTipDelay;
        }
        else
        {
            _caption = window.caption;
            _toolTipDelay = 500;
        }

        Vector2 mousePos = GetCurrentMousePosition();

        _mouse.X = mousePos.X;
        _mouse.Y = mousePos.Y;

        GetMousePositionRelativeTo(window, _mouse, ref _pointerOffset);

        StartTimer();

        return base.Begin(window, flags);
    }

    /// @see WindowToolTipAgent.as::end
    public override IWindow? End(IWindow? window)
    {
        StopTimer();
        HideToolTip();

        return base.End(window);
    }

    /// @see WindowToolTipAgent.as::operate
    public override void Operate(int mouseX, int mouseY)
    {
        if (_window == null || _window.disposed)
        {
            return;
        }

        _mouse.X = mouseX;
        _mouse.Y = mouseY;

        GetMousePositionRelativeTo(_window, _mouse, ref _pointerOffset);

        if (_tooltipWindow is not { disposed: false })
        {
            return;
        }

        _tooltipWindow.x = mouseX + _toolTipOffset.X;
        _tooltipWindow.y = mouseY + _toolTipOffset.Y;
    }

    /// @see WindowToolTipAgent.as::showToolTip
    protected void ShowToolTip()
    {
        StopTimer();

        if (_window == null || _window.disposed)
        {
            return;
        }

        // Refresh caption
        if (_window is IInteractiveWindow interactive)
        {
            _caption = interactive.ToolTipCaption;
        }
        else
        {
            _caption = _window.caption;
        }

        if (_tooltipWindow == null || _tooltipWindow.disposed)
        {
            // @see WindowToolTipAgent.as::showToolTip — use _window.context first, fallback to injected
            IWindowContext? ctx = _window.context ?? _windowContext;

            if (ctx != null)
            {
                // @see WindowToolTipAgent.as — pass all 11 args to Create including null parent, null properties
                _tooltipWindow = ctx.Create(
                    _window.name + "::ToolTip",
                    _caption ?? "",
                    Class3409.WINDOW_TYPE_TOOLTIP,
                    _window.style,
                    32,
                    default,
                    null,
                    null,
                    0,
                    null,
                    ""
                );
            }
        }

        if (_tooltipWindow == null)
        {
            return;
        }

        Vector2 globalPos = _window is WindowController wc2 ? wc2.GetGlobalPosition() : Vector2.Zero;
        _tooltipWindow.x = globalPos.X + _pointerOffset.X + _toolTipOffset.X;
        _tooltipWindow.y = globalPos.Y + _pointerOffset.Y + _toolTipOffset.Y;
        _tooltipWindow.visible = (_tooltipWindow.caption?.Length ?? 0) > 0;
    }

    /// @see WindowToolTipAgent.as::hideToolTip
    protected void HideToolTip()
    {
        if (_tooltipWindow is not { disposed: false })
        {
            return;
        }

        _tooltipWindow.Destroy();
        _tooltipWindow = null;
    }

    /// @see WindowToolTipAgent.as::updateCaption
    public void UpdateCaption(IWindow? window)
    {
        if (window == null || window.disposed || _tooltipWindow == null || _tooltipWindow.disposed)
        {
            return;
        }

        string? newCaption;
        if (window is IInteractiveWindow interactive)
        {
            newCaption = interactive.ToolTipCaption;
        }
        else
        {
            newCaption = window.caption;
        }

        if (newCaption == _caption)
        {
            return;
        }

        _caption = newCaption;

        if (string.IsNullOrEmpty(newCaption))
        {
            _tooltipWindow.visible = false;
        }
        else
        {
            _tooltipWindow.caption = newCaption!;
            _tooltipWindow.visible = true;
        }
    }

    private void StartTimer()
    {
        StopTimer();

        // Godot adaptation: use a one-shot callable delay
        // We store the timer reference so we can cancel it
        // TODO: SceneTreeTimer can't be reliably cancelled; future fix to use Timer node
        if (Engine.GetMainLoop() is not SceneTree tree)
        {
            return;
        }

        _timer = tree.CreateTimer(_toolTipDelay / 1000.0);
        _timer.Timeout += ShowToolTip;
    }

    private void StopTimer()
    {
        if (_timer == null)
        {
            return;
        }

        // Disconnect if still valid
        if (_timer.IsConnected("timeout", Callable.From(ShowToolTip)))
        {
            _timer.Timeout -= ShowToolTip;
        }

        _timer = null;
    }
}
