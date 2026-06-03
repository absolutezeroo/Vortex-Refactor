// @see habbo/window/HintManager.as

using System.Diagnostics;

using Godot;

using Vortex.Core.Runtime;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Motion;

namespace Vortex.Habbo.Window;

/// <summary>
/// Manages hint arrow animations that point at registered windows.
/// Implements IUpdateReceiver for ongoing position tracking with bobbing animation.
/// </summary>
/// @see habbo/window/HintManager.as
public class HintManager : IUpdateReceiver
{
    private const int HINT_MARGIN = 10;
    private const int HINT_INITIAL_OFFSET = 400;
    private const int HINT_STEP = 15;

    private HabboWindowManagerComponent? _windowManager;
    private Dictionary<string, HintTarget>? _registeredWindows;
    private HintTarget? _activeHint;
    private IWindow? _hint;
    private Rect2? _animationSource;
    private Rect2? _targetRect;

    /// @see HintManager.as::HintManager
    public HintManager(HabboWindowManagerComponent windowManager)
    {
        _registeredWindows = new Dictionary<string, HintTarget>(System.StringComparer.Ordinal);
        _windowManager = windowManager;
    }

    /// @see HintManager.as::get disposed
    public bool disposed => _windowManager == null;

    /// @see HintManager.as::get activeKey
    public string? ActiveKey => _activeHint?.Key;

    /// @see HintManager.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        HideHint();
        _activeHint = null;
        _registeredWindows = null;
        _windowManager = null;
    }

    /// @see HintManager.as::registerWindow
    public void RegisterWindow(string key, IWindow window, int style)
    {
        if (_registeredWindows == null)
        {
            return;
        }

        if (_registeredWindows.ContainsKey(key))
        {
            UnregisterWindow(key);
        }

        _registeredWindows[key] = new HintTarget(window, key, style);
    }

    /// @see HintManager.as::unregisterWindow
    public void UnregisterWindow(string key)
    {
        if (_registeredWindows == null)
        {
            return;
        }

        if (ActiveKey == key)
        {
            HideHint();
        }

        _registeredWindows.Remove(key);
    }

    /// @see HintManager.as::showHint
    public void ShowHint(string key, Rect2? sourceRect = null)
    {
        if (_registeredWindows == null || _windowManager == null)
        {
            return;
        }

        if (!_registeredWindows.TryGetValue(key, out HintTarget? target))
        {
            return;
        }

        if (target.Window == null || key == ActiveKey)
        {
            return;
        }

        HideHint();

        // @see HintManager.as — create static bitmap wrapper (type 23) for the hint arrow
        IWindowContext? context = (target.Window as WindowController)?.context;
        _hint = context?.Create("", "", 23, 0, 0, default, null, null, 0);

        switch (_hint)
        {
            case null:
                return;
            case IStaticBitmapWrapperWindow bmpWindow:
                // TODO(window-port): Set fitSizeToContents when property is available on IStaticBitmapWrapperWindow
                switch (target.Style - 1)
                {
                    case 0:
                        bmpWindow.AssetUri = "common_green_arrow_vertical";
                        break;
                    default:
                        bmpWindow.AssetUri = "common_green_arrow_horizontal";
                        break;
                }
                break;
        }

        _activeHint = target;
        _animationSource = sourceRect;
        _targetRect = GetTargetRect(_activeHint.Window);

        if (sourceRect.HasValue)
        {
            AnimateHint(sourceRect.Value);
        }
        else
        {
            _windowManager?.RegisterUpdateReceiver(this, 0);
            Update(0);
        }
    }

    /// @see HintManager.as::animateHint
    protected void AnimateHint(Rect2 source)
    {
        if (_hint == null || !_targetRect.HasValue)
        {
            return;
        }

        _hint.x = source.Position.X;
        _hint.y = source.Position.Y;
        _hint.visible = true;

        float dx = source.Position.X - _targetRect.Value.Position.X;
        float dy = source.Position.Y - _targetRect.Value.Position.Y;
        float distance = System.MathF.Sqrt((dx * dx) + (dy * dy));
        int duration = 500 - (int)System.MathF.Abs(1f / distance * 100f * 500f * 0.5f);

        int targetW = (int)_hint.width;
        int targetH = (int)_hint.height;
        _hint.width *= 0.4f;
        _hint.height *= 0.4f;

        Queue motion = new(
            new Combo(
                new EaseOut(new MoveTo(_hint, duration, (int)_targetRect.Value.Position.X, (int)_targetRect.Value.Position.Y), 1),
                new ResizeTo(_hint, duration, targetW, targetH)
            ),
            new Callback(MotionComplete)
        );

        MotionManager.RunMotion(motion);
    }

    /// @see HintManager.as::motionComplete
    protected void MotionComplete(Motion motion)
    {
        _windowManager?.RegisterUpdateReceiver(this, 0);

        Update(0);
    }

    /// @see HintManager.as::hideHint
    public void HideHint()
    {
        _windowManager?.RemoveUpdateReceiver(this);

        _activeHint = null;

        if (_hint == null)
        {
            return;
        }

        _hint.Destroy();
        _hint = null;
    }

    /// @see HintManager.as::hideMatchingHint
    public void HideMatchingHint(string key)
    {
        if (key == ActiveKey)
        {
            HideHint();
        }
    }

    /// @see HintManager.as::update
    public void Update(uint param1)
    {
        if (_activeHint?.Window == null || _hint == null)
        {
            return;
        }

        Vector2 globalPos = _activeHint.Window.GetGlobalPosition();

        if (globalPos == Vector2.Zero)
        {
            return;
        }

        float elapsed = Stopwatch.GetTimestamp() / (float)Stopwatch.Frequency * 1000f;
        float bob = 5f * System.MathF.Abs(System.MathF.Sin(elapsed * 0.003f));

        // @see HintManager.as — get desktop width for horizontal positioning
        float desktopWidth = _activeHint.Window.parent?.width ?? 0;

        switch (_activeHint.Style - 1)
        {
            case 0: // vertical arrow
                if (globalPos.Y - _hint.height - HINT_MARGIN > 0)
                {
                    _hint.y = globalPos.Y - _hint.height - HINT_MARGIN - bob;
                }
                else
                {
                    _hint.y = globalPos.Y + _activeHint.Window.height + HINT_MARGIN + bob;
                }
                _hint.x = globalPos.X + ((_activeHint.Window.width - _hint.width) / 2f);
                break;
            default: // horizontal arrow
                if (globalPos.X + (_activeHint.Window.width / 2f) > desktopWidth / 2f)
                {
                    _hint.x = globalPos.X - _hint.width - HINT_MARGIN - bob;
                }
                else
                {
                    _hint.x = globalPos.X + _activeHint.Window.width + HINT_MARGIN + bob;
                }
                _hint.y = globalPos.Y + ((_activeHint.Window.height - _hint.height) / 2f);
                break;
        }

        _hint.visible = _activeHint.Window.visible;
    }

    /// @see HintManager.as::getTargetRect
    private Rect2 GetTargetRect(IWindow window)
    {
        Vector2 globalPos = window.GetGlobalPosition();

        float tx = 0,
            ty = 0;

        if (_activeHint == null || _hint == null)
        {
            return new Rect2(tx, ty, 0, 0);
        }

        // @see HintManager.as — get desktop width for horizontal positioning
        float desktopWidth = window.parent?.width ?? 0;

        switch (_activeHint.Style - 1)
        {
            case 0:
                ty = globalPos.Y - _hint.height - HINT_MARGIN > 0
                    ? globalPos.Y - _hint.height - HINT_MARGIN
                    : globalPos.Y + window.height + HINT_MARGIN;
                tx = globalPos.X + ((window.width - _hint.width) / 2f);
                break;
            default:
                tx = globalPos.X + (window.width / 2f) > desktopWidth / 2f
                    ? globalPos.X - _hint.width - HINT_MARGIN
                    : globalPos.X + window.width + HINT_MARGIN;
                ty = globalPos.Y + ((window.height - _hint.height) / 2f);
                break;
        }

        return new Rect2(tx, ty, 0, 0);
    }
}
