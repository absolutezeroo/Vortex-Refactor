// @see core/window/components/TextLabelController.as

using System;

using Godot;

using Vortex.Core.Localization;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Theme;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/TextLabelController.as
public class TextLabelController : WindowController, ITextFieldContainer, ILocalizable
{
    private uint? _textColor;
    private bool _refreshing;
    private bool _vertical;
    private bool _localized;
    private TextMargins? _margins;

    /// @see TextLabelController.as::TextLabelController (default)
    public TextLabelController() : base() { }

    /// @see TextLabelController.as::TextLabelController (name + rect)
    public TextLabelController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see TextLabelController.as::TextLabelController (full AS3 11-param signature)
    public TextLabelController
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
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see TextLabelController.as — resolve default text_style from theme defaults
        // AS3: _textStyleName = String(param5.getWindowFactory().getThemeManager().getPropertyDefaults(param3).method_20("text_style"))
        try
        {
            IWindowFactory factory = param5.GetWindowFactory();

            {
                IThemeManager? themeManager = factory.GetThemeManager();
                IPropertyMap? propDefaults = themeManager?.GetPropertyDefaults(param3);
                PropertyStruct? textStyleProp = propDefaults?.GetValue("text_style");

                if (textStyleProp?.value is string styleName && !string.IsNullOrEmpty(styleName))
                {
                    TextStyleName = styleName;
                }
            }
        }
        catch
        {
            // Fallback: keep default "u_regular"
        }

        // @see TextLabelController.as — listen for style changes
        TextStyleManager.StyleChanged += OnTextStyleChanged;
    }

    /// @see TextLabelController.as::get text
    public string text { get; private set; } = "";

    /// @see TextLabelController.as::set text
    public void SetText(string? value)
    {
        if (value == null)
        {
            return;
        }

        // @see TextLabelController.as — remove old localization listener if localized
        if (_localized)
        {
            _context?.RemoveLocalizationListener(ExtractLocalizationKey(_caption), this);
            _localized = false;
        }

        _caption = value;

        // @see TextLabelController.as — detect ${key} localization pattern
        if (value.Length > 2 && value[0] == '$' && value[1] == '{')
        {
            _context?.RegisterLocalizationListener(ExtractLocalizationKey(value), this);
            _localized = true;
        }
        else
        {
            text = value;
            Refresh();
        }
    }

    /// @see TextLabelController.as::set localization
    public void SetLocalization(string? value)
    {
        if (value == null)
        {
            return;
        }

        text = value;
        Refresh();
    }

    /// @see TextLabelController.as::set caption — delegates to text setter
    public override string caption
    {
        get => _caption;
        set => SetText(value);
    }

    /// @see TextLabelController.as::get textStyle
    public TextStyle? TextStyle => TextStyleManager.GetStyle(TextStyleName);

    /// @see TextLabelController.as::set textStyle
    public void SetTextStyle(TextStyle? value)
    {
        if (value?.Name == null)
        {
            return;
        }
        if (TextStyleName == value.Name)
        {
            return;
        }

        TextStyleName = value.Name;
        Refresh();
    }

    /// @see TextLabelController.as::get textColor
    public uint TextColorValue => _textColor ?? 0;

    /// @see TextLabelController.as::set textColor
    public void SetTextColor(uint value)
    {
        if (_textColor == value)
        {
            return;
        }

        _textColor = value;

        Refresh();
    }

    /// @see TextLabelController.as::get hasTextColor
    public bool HasTextColor => _textColor != null;

    /// @see TextLabelController.as::get textWidth
    public float TextWidth { get; private set; }

    /// @see TextLabelController.as::get textHeight
    public float TextHeight { get; private set; }

    /// @see TextLabelController.as::get textStyleName (used by LabelRenderer cache key)
    public string? TextStyleName { get; private set; } = "u_regular";

    /// @see TextLabelController.as::get drawOffsetX
    public int DrawOffsetX => _margins != null ? _margins.LeftValue : 0;

    /// @see TextLabelController.as::get drawOffsetY
    public int DrawOffsetY => _margins != null ? _margins.TopValue : 0;

    /// @see TextLabelController.as::get margins
    public IMargins Margins
    {
        get
        {
            _margins ??= new TextMargins(0, 0, 0, 0, SetTextMargins);

            return _margins;
        }
    }

    /// @see TextLabelController.as::get length
    public int Length => text.Length;

    /// @see TextLabelController.as::get vertical
    public bool Vertical
    {
        get => _vertical;
        set
        {
            _vertical = value;

            Refresh();
        }
    }

    /// @see ITextFieldContainer.as::get textField
    object? ITextFieldContainer.TextField => TextFieldCache.GetByStyleName(TextStyleName);

    /// @see ITextFieldContainer.as::get margins
    IMargins? ITextFieldContainer.Margins => _margins;

    /// @see TextLabelController.as::set properties
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        foreach (PropertyStruct prop in properties)
        {
            switch (prop.key)
            {
                case "text_style":
                    if (prop.value is string styleName)
                    {
                        TextStyle? style = TextStyleManager.GetStyle(styleName);
                        if (style != null)
                        {
                            SetTextStyle(style);
                        }
                    }
                    break;

                case "text_color":
                    _textColor = prop.value switch
                    {
                        uint colorVal => colorVal,
                        int colorInt => (uint)colorInt,
                        _ => _textColor,
                    };
                    break;

                case "margin_left":
                    if ((_margins != null || prop.valid) && prop.value != null)
                    {
                        Margins.Left = Convert.ToInt32(prop.value);
                    }
                    break;

                case "margin_top":
                    if ((_margins != null || prop.valid) && prop.value != null)
                    {
                        Margins.Top = Convert.ToInt32(prop.value);
                    }
                    break;

                case "margin_right":
                    if ((_margins != null || prop.valid) && prop.value != null)
                    {
                        Margins.Right = Convert.ToInt32(prop.value);
                    }
                    break;

                case "margin_bottom":
                    if ((_margins != null || prop.valid) && prop.value != null)
                    {
                        Margins.Bottom = Convert.ToInt32(prop.value);
                    }
                    break;

                case "margins":
                    if (prop.value is IDictionary<string, object?> map)
                    {
                        SetTextMarginMap(map);
                    }
                    break;

                case "vertical":
                    if (prop.value is bool v)
                    {
                        Vertical = v;
                    }
                    break;
            }
        }

        base.ApplyProperties(properties);
    }

    /// @see TextLabelController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        TextStyleManager.StyleChanged -= OnTextStyleChanged;

        if (_localized)
        {
            _localized = false;
            _context?.RemoveLocalizationListener(ExtractLocalizationKey(_caption), this);
        }

        _margins?.Dispose();
        _margins = null;

        return base.Destroy();
    }

    /// @see TextLabelController.as::refresh
    private void Refresh(bool suppressEvent = false)
    {
        if (_refreshing)
        {
            return;
        }

        _refreshing = true;

        TextFieldCache.CachedFontEntry? fontEntry = TextFieldCache.GetByStyleName(TextStyleName);

        if (fontEntry == null)
        {
            _refreshing = false;
            return;
        }

        // Measure text
        Vector2 textSize = fontEntry.Font.GetStringSize(
            text,
            HorizontalAlignment.Left,
            -1,
            fontEntry.FontSize
        );

        TextWidth = textSize.X;
        TextHeight = textSize.Y;

        // @see TextLabelController.as::refresh — auto-resize window to fit text + margins
        int marginH = _margins != null ? _margins.LeftValue + _margins.RightValue : 0;
        int marginV = _margins != null ? _margins.TopValue + _margins.BottomValue : 0;
        int availW = (int)width - marginH;
        int measuredW = (int)Math.Floor(TextWidth);
        int measuredH = (int)Math.Floor(TextHeight);
        bool resized = false;

        if (_vertical)
        {
            if (measuredW != (int)height - marginV)
            {
                SetRectangle(x, y, measuredH + marginH, measuredW + marginV);
                resized = true;
            }
            if (measuredH < availW)
            {
                // TextField height fits
            }
            else if (measuredH > availW)
            {
                SetRectangle(x, y, measuredH + marginH, measuredW + marginV);
                resized = true;
            }
        }
        else
        {
            if (measuredW != availW)
            {
                SetRectangle(x, y, measuredW + marginH, measuredH + marginV);
                resized = true;
            }

            int availH = (int)height - marginV;

            if (measuredH > availH)
            {
                SetRectangle(x, y, measuredW + marginH, measuredH + marginV);
                resized = true;
            }
        }

        _refreshing = false;

        // Invalidate for redraw
        _context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);

        // @see TextLabelController.as — dispatch WE_RESIZED to parent if size didn't change
        if (resized || suppressEvent || _parent is not WindowController parentWc)
        {
            return;
        }

        WindowEvent evt = WindowEvent.Allocate(WindowEvent.WE_RESIZED, this, null);

        parentWc.NotifyEventListeners(evt);
    }

    /// @see TextLabelController.as::setTextMargins
    private void SetTextMargins(IMargins? value)
    {
        if (value != null && value != _margins)
        {
            if (_margins != null)
            {
                _margins.Assign(
                    value.Left,
                    value.Top,
                    value.Right,
                    value.Bottom,
                    SetTextMargins
                );
            }
            else
            {
                _margins = new TextMargins(
                    value.Left,
                    value.Top,
                    value.Right,
                    value.Bottom,
                    SetTextMargins
                );
            }
        }

        Refresh();
    }

    /// @see TextLabelController.as::setTextMarginMap — accepts Map from XML properties
    private void SetTextMarginMap(IDictionary<string, object?> map)
    {
        int left = map.TryGetValue("left", out object? lv) ? Convert.ToInt32(lv) : 0;
        int top = map.TryGetValue("top", out object? tv) ? Convert.ToInt32(tv) : 0;
        int right = map.TryGetValue("right", out object? rv) ? Convert.ToInt32(rv) : 0;
        int bottom = map.TryGetValue("bottom", out object? bv) ? Convert.ToInt32(bv) : 0;

        if (_margins != null)
        {
            _margins.Assign(left, top, right, bottom, SetTextMargins);
        }
        else
        {
            _margins = new TextMargins(left, top, right, bottom, SetTextMargins);
        }

        Refresh();
    }

    /// @see TextLabelController.as — extracts key from "${key}" pattern: slice(2, indexOf("}"))
    private static string ExtractLocalizationKey(string caption)
    {
        int endIndex = caption.IndexOf('}');

        return endIndex > 2 ? caption[2..endIndex] : caption[2..];
    }

    /// @see TextLabelController.as::onTextStyleChanged
    private void OnTextStyleChanged()
    {
        TextImageRenderer.InvalidateCache(TextStyleName);

        Refresh();
    }
}
