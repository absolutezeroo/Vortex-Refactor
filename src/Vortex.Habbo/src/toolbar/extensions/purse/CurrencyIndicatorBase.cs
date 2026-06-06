// @see com.sulake.habbo.toolbar.extensions.purse.CurrencyIndicatorBase

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Habbo.Window;
using Vortex.Habbo.Window.Utils;

namespace Vortex.Habbo.Toolbar.Extensions.Purse;

/// @see com.sulake.habbo.toolbar.extensions.purse.CurrencyIndicatorBase
public abstract class CurrencyIndicatorBase : ICurrencyIndicator
{
    /// @see CurrencyIndicatorBase.as::const_854, const_579 — animation direction constants
    protected const int ANIM_FORWARD = 0;
    protected const int ANIM_REVERSE = 1;

    private const float OVERLAY_PHASE_STEP = 0.025f;

    protected IWindowContainer? _window;
    protected readonly IHabboWindowManager _windowManager;
    protected readonly IAssetLibrary? _assets;
    private bool _disposed;

    private IStaticBitmapWrapperWindow? _iconBmp;
    private uint _bgColorLight;
    private uint _bgColorDark;
    private string? _textElementName;
    private uint _iconAnimationDelay;
    private string? _amountZeroText;
    private readonly List<string> _animFrames = new();
    private int _animDir;
    private int _animOffset;

    // @see CurrencyIndicatorBase.as — overlay animation state
    private float _overlayPhase;
    private int _overlayStartValue;
    private int _overlayEndValue;

    /// @see CurrencyIndicatorBase.as::CurrencyIndicatorBase
    protected CurrencyIndicatorBase(IHabboWindowManager windowManager, IAssetLibrary? assets)
    {
        _windowManager = windowManager;
        _assets = assets;
    }

    protected uint bgColorLight { set => _bgColorLight = value; }
    protected uint bgColorDark { set => _bgColorDark = value; }
    protected string? textElementName { get => _textElementName; set => _textElementName = value; }
    protected uint iconAnimationDelay { set => _iconAnimationDelay = value; }

    protected string? amountZeroText
    {
        get => _amountZeroText;
        set => _amountZeroText = value;
    }

    /// @see CurrencyIndicatorBase.as::iconAnimationSequence (set)
    protected string[] iconAnimationSequence
    {
        set => _animFrames.AddRange(value);
    }

    /// @see ICurrencyIndicator::get window
    public IWindowContainer? window => _window;

    /// @see ICurrencyIndicator::dispose
    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_window != null)
        {
            _window.Destroy();
            _window = null;
        }

        _disposed = true;
    }

    /// @see ICurrencyIndicator::registerUpdateEvents
    public virtual void RegisterUpdateEvents(object? eventDispatcher)
    {
        // overridden in subclasses
    }

    public virtual void UnregisterUpdateEvents(object? eventDispatcher)
    {
    }

    /// @see CurrencyIndicatorBase.as::onContainerClick
    protected virtual void OnContainerClick(WindowEvent ev, IWindow window) { }

    /// @see CurrencyIndicatorBase.as::createWindow
    protected void CreateWindow(string xmlAssetName, string? iconBitmapName)
    {
        XmlAsset? xmlAsset = _assets?.GetAssetByName(xmlAssetName) as XmlAsset;
        System.Xml.Linq.XElement? layoutXml = xmlAsset?.Content as System.Xml.Linq.XElement
            ?? HabboAssetResolver.LoadXmlAsset(xmlAssetName);

        if (layoutXml == null)
        {
            return;
        }

        _window = _windowManager.BuildFromXml(layoutXml, 1) as IWindowContainer;

        if (_window == null)
        {
            return;
        }

        _window.AddEventListener(WindowMouseEvent.CLICK, OnContainerClick);
        _window.AddEventListener(WindowMouseEvent.OVER, OnContainerMouseOver);
        _window.AddEventListener(WindowMouseEvent.OUT, OnContainerMouseOut);

        if (iconBitmapName != null)
        {
            List<IWindow> iconWindows = new();
            _window.GroupChildrenWithTag("ICON", iconWindows, -1);

            if (iconWindows.Count == 1 && iconWindows[0] is IStaticBitmapWrapperWindow sbw)
            {
                _iconBmp = sbw;
                SetIconBitmap(iconBitmapName);
            }
        }
    }

    /// @see CurrencyIndicatorBase.as::animateIcon
    protected void AnimateIcon(int direction)
    {
        // TODO(as3-port): Timer-based icon animation — simplified; call SetIconBitmap directly for now
        _animDir = direction;

        if (_iconBmp != null && _animFrames.Count > 0)
        {
            SetIconBitmap(_animFrames[0]);
        }
    }

    /// @see CurrencyIndicatorBase.as::setAmount
    protected virtual void SetAmount(int amount, int minutes = -1)
    {
        SetText(amount.ToString());
    }

    /// @see CurrencyIndicatorBase.as::setText
    protected void SetText(string text)
    {
        if (_window != null && _textElementName != null)
        {
            IWindow? label = _window.FindChildByName(_textElementName);
            if (label != null)
            {
                label.caption = text;
            }
        }
    }

    /// @see CurrencyIndicatorBase.as::setTextUnderline
    protected void SetTextUnderline(bool underline)
    {
        if (_window != null && _textElementName != null)
        {
            if (_window.FindChildByName(_textElementName) is ITextWindow tw)
            {
                tw.Underline = underline;
            }
        }
    }

    /// @see CurrencyIndicatorBase.as::animateChange
    protected void AnimateChange(int fromValue, int toValue)
    {
        // @see CurrencyIndicatorBase.as::animateChange — overlay animation
        _overlayPhase = 0f;
        _overlayStartValue = fromValue;
        _overlayEndValue = toValue;

        IWindow? change = _window?.FindChildByName("change");
        if (change != null)
        {
            int delta = toValue - fromValue;
            change.caption = (delta > 0 ? "+" : "") + delta.ToString();
        }

        // TODO(as3-port): start timer for overlay animation (40ms ticks)
        // For now, snap to final value
        SetAmount(toValue);
        HideOverlay();
    }

    private void HideOverlay()
    {
        IWindow? overlay = _window?.FindChildByName("change_overlay");
        if (overlay != null)
        {
            overlay.visible = false;
        }
    }

    private void SetIconBitmap(string assetUri)
    {
        if (_iconBmp != null)
        {
            _iconBmp.AssetUri = assetUri;
        }
    }

    private void OnContainerMouseOver(WindowEvent ev, IWindow window)
    {
        IWindow? bg = _window?.FindChildByTag("BGCOLOR");
        if (bg != null)
        {
            bg.color = _bgColorLight;
        }
    }

    private void OnContainerMouseOut(WindowEvent ev, IWindow window)
    {
        IWindow? bg = _window?.FindChildByTag("BGCOLOR");
        if (bg != null)
        {
            bg.color = _bgColorDark;
        }
    }
}
