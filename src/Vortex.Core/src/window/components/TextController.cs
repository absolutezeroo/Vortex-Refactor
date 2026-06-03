// @see core/window/components/TextController.as

using System;

using Godot;

using Vortex.Core.Localization;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Theme;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/TextController.as
public class TextController : WindowController, ITextWindow, ITextFieldContainer, ILocalizable
{
    /// @see TextController.as::REPLACE_RANDOM_CHARS
    private static readonly string[] REPLACE_RANDOM_CHARS = ["a", "B", "c", "D", "e"];

    /// @see TextController.as::const_429 — property name → setter table
    private static readonly Dictionary<string, Action<TextController, object?>> _propertySetterTable = CreatePropertySetterTable();

    private TextStyle _explicitStyle = new();
    private TextMargins? _margins;
    private bool _drawing;
    protected bool _localized;
    private readonly bool _settingRectangle;

    // Godot text state (replaces Flash _field)
    private string _text = "";

    // Measured text dimensions

    /// @see TextController.as::TextController (default)
    public TextController() : base() { }

    /// @see TextController.as::TextController (name + rect)
    public TextController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see TextController.as::TextController (full AS3 11-param signature)
    public TextController
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
        // @see TextController.as — initialize explicit style and margins
        _explicitStyle = new TextStyle();
        _margins = new TextMargins(0, 0, 0, 0, OnSetTextMargins);
        _drawing = false;
        ScrollH = 0;
        ScrollV = 0;

        // @see TextController.as — resolve default text_style from theme
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
            // Fallback: keep default "regular"
        }

        // @see TextController.as — apply text formatting from CSS style
        ApplyTextFormatting();

        // @see TextController.as — listen for CSS changes
        TextStyleManager.StyleChanged += OnTextStyleChanged;
    }

    /// @see TextController.as::get text
    public virtual new string text => _text;

    /// @see TextController.as::get htmlText
    public string htmlText { get; private set; } = "";

    /// @see TextController.as::get textColor
    public uint textColor { get; private set; }

    /// @see TextController.as::get textStyle
    public TextStyle? textStyle => TextStyleManager.GetStyle(TextStyleName);

    /// @see TextController.as::get textStyleName
    public string TextStyleName { get; private set; } = "regular";

    /// @see TextController.as::get autoSize
    public string autoSize { get; private set; } = "none";

    /// @see TextController.as::get margins
    public IMargins TextMargins => _margins ??= new TextMargins(0, 0, 0, 0, OnSetTextMargins);

    /// @see TextController.as::get etchingColor
    public uint etchingColor { get; private set; }

    /// @see TextController.as::get etchingPosition
    public string? etchingPosition { get; private set; }

    /// @see TextController.as::get bold
    public bool Bold { get; private set; }

    /// @see TextController.as::get italic
    public bool Italic { get; private set; }

    /// @see TextController.as::get underline
    public bool Underline { get; private set; }

    /// @see TextController.as::get fontFace
    public string FontFace { get; private set; } = "Ubuntu";

    /// @see TextController.as::get fontSize
    public int FontSize { get; private set; } = 12;

    /// @see TextController.as::get wordWrap
    public bool WordWrap { get; private set; }

    /// @see TextController.as::get multiline
    public bool Multiline { get; private set; }

    /// @see TextController.as::get maxChars
    public int MaxChars { get; private set; }

    /// @see TextController.as::get border
    public bool Border { get; private set; }

    /// @see TextController.as::get borderColor
    public uint BorderColor { get; private set; }

    /// @see TextController.as::get textBackground
    public bool TextBackground { get; private set; }

    /// @see TextController.as::get textBackgroundColor
    public uint TextBackgroundColor { get; private set; }

    /// @see TextController.as::get textWidth
    public float TextWidth { get; private set; }

    /// @see TextController.as::get textHeight
    public float TextHeight { get; private set; }

    /// @see TextController.as::get length
    public int Length => _text.Length;

    /// @see TextController.as::get numLines
    public int NumLines => Math.Max(1, _text.Split('\n').Length);

    /// @see TextController.as::get spacing
    public int Spacing { get; private set; }

    /// @see TextController.as::get leading
    public int Leading { get; private set; }

    /// @see TextController.as::get kerning
    public bool Kerning { get; private set; }

    /// @see TextController.as::get maxLines
    public int MaxLines { get; private set; }

    /// @see TextController.as::get sharpness
    public int Sharpness { get; private set; }

    /// @see TextController.as::get thickness
    public int Thickness { get; private set; }

    /// @see TextController.as::get embedFonts
    public bool EmbedFonts { get; private set; }

    /// @see TextController.as::get antiAliasType
    public string AntiAliasType { get; private set; } = "advanced";

    /// @see TextController.as::get gridFitType
    public string GridFitType { get; private set; } = "pixel";

    /// @see TextController.as::get restrict
    public string Restrict { get; private set; } = "";

    /// @see TextController.as::get overflowReplace
    public string OverflowReplace { get; private set; } = "";

    /// @see TextController.as::get isOverflowReplaceOn
    public bool IsOverflowReplaceOn => !string.IsNullOrEmpty(OverflowReplace);

    /// @see TextController.as::get isOverflown
    public bool IsOverflown { get; private set; }

    /// @see TextController.as::get scrollH
    public float ScrollH { get; private set; }

    /// @see TextController.as::get scrollV
    public float ScrollV { get; private set; }

    /// @see TextController.as::get maxScrollH
    public float MaxScrollH => Math.Max(0, TextWidth - width);

    /// @see TextController.as::get maxScrollV
    public float MaxScrollV => Math.Max(0, TextHeight - height);

    /// @see TextController.as::get bottomScrollV — last visible line (1-based)
    public int BottomScrollV
    {
        get
        {
            TextFieldCache.CachedFontEntry? fontEntry = TextFieldCache.GetByStyleName(TextStyleName);

            if (fontEntry == null)
            {
                return 1;
            }

            float lineH = fontEntry.Font.GetHeight(fontEntry.FontSize) + Leading;

            if (lineH <= 0)
            {
                return 1;
            }

            int visibleLines = (int)(height / lineH);
            int scrollLine = (int)(ScrollV / lineH);

            return Math.Min(scrollLine + visibleLines, NumLines);
        }
    }

    /// @see TextController.as::get visibleRegion
    public Rect2 VisibleRegion => new(0, 0, width, height);

    /// @see TextController.as::get scrollableRegion
    public Rect2 ScrollableRegion => new(0, 0, TextWidth, TextHeight);

    /// @see TextController.as::get scrollStepH
    public float ScrollStepH { get; set; } = 1f;

    /// @see TextController.as::get scrollStepV
    public float ScrollStepV { get; set; } = 1f;

    /// @see TextController.as::set text
    public virtual void SetText(string? value)
    {
        if (value == null)
        {
            return;
        }

        // @see TextController.as — remove old localization listener if localized
        if (_localized)
        {
            _context?.RemoveLocalizationListener(ExtractLocalizationKey(_caption), this);
            _localized = false;
        }

        _caption = value;

        // @see TextController.as — detect ${key} localization pattern
        if (value.Length > 2 && value[0] == '$' && value[1] == '{')
        {
            _localized = true;
            _context?.RegisterLocalizationListener(ExtractLocalizationKey(value), this);
        }
        else
        {
            _text = value;
            RefreshTextImage();
        }
    }

    /// @see TextController.as::set caption — delegates to text
    public override string caption
    {
        get => _caption;
        set => SetText(value);
    }

    /// @see TextController.as::set htmlText
    public virtual void SetHtmlText(string? value)
    {
        if (value == null)
        {
            return;
        }

        _localized = false;
        _caption = value;
        htmlText = value;
        _text = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set textColor
    public virtual void SetTextColor(uint value)
    {
        textColor = value;
        _explicitStyle.Color = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set textStyle
    public virtual void SetTextStyle(TextStyle? value)
    {
        if (value?.Name == null)
        {
            return;
        }

        TextStyle? resolved = TextStyleManager.GetStyle(value.Name);

        if (resolved == null)
        {
            return;
        }

        TextStyleName = resolved.Name!;

        ApplyTextFormatting();
        RefreshTextImage();
    }

    /// @see TextController.as::setTextStyleString
    public virtual void SetTextStyleString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        TextStyle? style = TextStyleManager.GetStyle(value);

        if (style == null)
        {
            return;
        }

        TextStyleName = style.Name!;

        ApplyTextFormatting();
        RefreshTextImage();
    }

    /// @see TextController.as::set autoSize
    public virtual void SetAutoSize(string? value)
    {
        if (value == null)
        {
            return;
        }

        if (autoSize == value)
        {
            return;
        }

        autoSize = value;
        RefreshTextImage();
    }

    /// @see TextController.as::set bold
    public virtual void SetBold(bool value)
    {
        Bold = value;
        _explicitStyle.FontWeight = value ? "bold" : "normal";

        RefreshTextImage();
    }

    /// @see TextController.as::set italic
    public virtual void SetItalic(bool value)
    {
        Italic = value;
        _explicitStyle.FontStyle = value ? "italic" : "normal";

        RefreshTextImage();
    }

    /// @see TextController.as::set underline
    public virtual void SetUnderline(bool value)
    {
        Underline = value;
        _explicitStyle.TextDecoration = value ? "underline" : "none";

        RefreshTextImage();
    }

    /// @see TextController.as::set fontFace
    public virtual void SetFontFace(string? value)
    {
        if (value == null)
        {
            return;
        }

        FontFace = value;
        _explicitStyle.FontFamily = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set fontSize
    public virtual void SetFontSize(int value)
    {
        FontSize = value;
        _explicitStyle.FontSize = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set wordWrap
    public virtual void SetWordWrap(bool value)
    {
        WordWrap = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set multiline
    public virtual void SetMultiline(bool value)
    {
        Multiline = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set maxChars
    public virtual void SetMaxChars(int value)
    {
        MaxChars = value;
    }

    /// @see TextController.as::set border
    public virtual void SetBorder(bool value)
    {
        Border = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set borderColor
    public virtual void SetBorderColor(uint value)
    {
        BorderColor = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set textBackground
    public virtual void SetTextBackground(bool value)
    {
        TextBackground = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set textBackgroundColor
    public virtual void SetTextBackgroundColor(uint value)
    {
        TextBackgroundColor = value;
        RefreshTextImage();
    }

    /// @see TextController.as::set etchingColor
    public virtual void SetEtchingColor(uint value)
    {
        etchingColor = value;
        _explicitStyle.EtchingColor = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set etchingPosition
    public virtual void SetEtchingPosition(string? value)
    {
        etchingPosition = value;
        _explicitStyle.EtchingPosition = value;
        RefreshTextImage();
    }

    /// @see TextController.as::set spacing
    public virtual void SetSpacing(int value)
    {
        Spacing = value;
        _explicitStyle.LetterSpacing = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set leading
    public virtual void SetLeading(int value)
    {
        Leading = value;
        _explicitStyle.Leading = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set kerning
    public virtual void SetKerning(bool value)
    {
        Kerning = value;
        _explicitStyle.Kerning = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set maxLines
    public virtual void SetMaxLines(int value)
    {
        MaxLines = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set sharpness
    public virtual void SetSharpness(int value)
    {
        Sharpness = value;
        _explicitStyle.Sharpness = value;
    }

    /// @see TextController.as::set thickness
    public virtual void SetThickness(int value)
    {
        Thickness = value;
        _explicitStyle.Thickness = value;
    }

    /// @see TextController.as::set embedFonts
    public virtual void SetEmbedFonts(bool value)
    {
        EmbedFonts = value;
    }

    /// @see TextController.as::set antiAliasType
    public virtual void SetAntiAliasType(string? value)
    {
        if (value != null)
        {
            AntiAliasType = value;
        }
    }

    /// @see TextController.as::set gridFitType
    public virtual void SetGridFitType(string? value)
    {
        if (value != null)
        {
            GridFitType = value;
        }
    }

    /// @see TextController.as::set restrict
    public virtual void SetRestrict(string? value)
    {
        Restrict = value ?? "";
    }

    /// @see TextController.as::set overflowReplace
    public virtual void SetOverflowReplace(string? value)
    {
        OverflowReplace = value ?? "";

        RefreshTextImage();
    }

    /// @see TextController.as::set scrollH
    public virtual void SetScrollH(float value)
    {
        ScrollH = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set scrollV
    public virtual void SetScrollV(float value)
    {
        ScrollV = value;

        RefreshTextImage();
    }

    /// @see TextController.as::set styleSheet
    // TODO(window-port): AS3 StyleSheet integration
    public virtual void SetStyleSheet(object? value)
    {
    }

    /// @see TextController.as::set localization
    public virtual void SetLocalization(string? value)
    {
        if (value == null)
        {
            return;
        }

        _text = value;

        RefreshTextImage();
    }

    /// @see TextController.as — ITextFieldContainer.textField
    object? ITextFieldContainer.TextField => TextFieldCache.GetByStyleName(TextStyleName);

    /// @see TextController.as — ITextFieldContainer.margins
    IMargins? ITextFieldContainer.Margins => _margins;

    /// @see TextController.as::refreshTextImage
    protected virtual void RefreshTextImage(bool suppressResizeEvent = false)
    {
        if (_drawing)
        {
            return;
        }

        _drawing = true;
        IsOverflown = false;

        // Measure text
        TextFieldCache.CachedFontEntry? fontEntry = TextFieldCache.GetByStyleName(TextStyleName);

        if (fontEntry != null)
        {
            float marginH = (_margins?.LeftValue ?? 0) + (_margins?.RightValue ?? 0);
            float marginV = (_margins?.TopValue ?? 0) + (_margins?.BottomValue ?? 0);
            float availW = width - marginH;
            float availH = height - marginV;

            // @see TextController.as — measure with word wrap
            int wrapWidth = WordWrap ? (int)Math.Max(availW, 1) : -1;

            Vector2 textSize = fontEntry.Font.GetStringSize(
                _text,
                HorizontalAlignment.Left,
                wrapWidth,
                fontEntry.FontSize
            );

            TextWidth = textSize.X;
            TextHeight = textSize.Y;

            // @see TextController.as — overflow replacement logic
            if (IsOverflowReplaceOn && autoSize is "none" or "right")
            {
                if (TextHeight + marginV > availH + marginV || TextWidth + marginH > availW + marginH)
                {
                    IsOverflown = true;
                }
            }

            // @see TextController.as — autoSize adjustments
            bool sizeChanged = false;

            if (autoSize is "left" or "center" or "right")
            {
                float newWidth = TextWidth + marginH;
                float newHeight = TextHeight + marginV;

                if (Math.Abs(width - newWidth) > 0.5f || Math.Abs(height - newHeight) > 0.5f)
                {
                    base.width = Math.Max(newWidth, 1);
                    base.height = Math.Max(newHeight, 1);
                    sizeChanged = true;
                }
            }

            // @see TextController.as — dispatch WE_RESIZED if needed
            if (sizeChanged && !suppressResizeEvent && _parent != null)
            {
                WindowEvent evt = WindowEvent.Allocate(WindowEvent.WE_RESIZED, this, null);

                _parent.NotifyEventListeners(evt);
            }
        }

        _drawing = false;

        // Invalidate for redraw
        _context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.REDRAW);
    }

    /// @see TextController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        if (_drawing)
        {
            return base.Update(param1, param2);
        }

        if (param2.type == WindowEvent.WE_RESIZED)
        {
            RefreshTextImage(true);
        }

        return base.Update(param1, param2);
    }

    /// @see TextController.as::appendText
    public virtual void AppendText(string? value)
    {
        if (value == null)
        {
            return;
        }

        _text += value;
        RefreshTextImage();
    }

    /// @see TextController.as::setTextMargins
    public virtual void SetTextMargins(int left, int top, int right, int bottom)
    {
        if (_margins != null)
        {
            _margins.Assign(left, top, right, bottom, OnSetTextMargins);
        }
        else
        {
            _margins = new TextMargins(left, top, right, bottom, OnSetTextMargins);
        }

        RefreshTextImage();
    }

    /// @see TextController.as::setTextMarginMap
    public virtual void SetTextMarginMap(IDictionary<string, object>? map)
    {
        if (map == null)
        {
            return;
        }

        int left = _margins?.LeftValue ?? 0;
        int top = _margins?.TopValue ?? 0;
        int right = _margins?.RightValue ?? 0;
        int bottom = _margins?.BottomValue ?? 0;

        if (map.TryGetValue("left", out object? l))
        {
            left = Convert.ToInt32(l);
        }

        if (map.TryGetValue("top", out object? t))
        {
            top = Convert.ToInt32(t);
        }

        if (map.TryGetValue("right", out object? r))
        {
            right = Convert.ToInt32(r);
        }

        if (map.TryGetValue("bottom", out object? b))
        {
            bottom = Convert.ToInt32(b);
        }

        SetTextMargins(left, top, right, bottom);
    }

    /// @see TextController.as::resetExplicitStyle
    public virtual void ResetExplicitStyle()
    {
        _explicitStyle = new TextStyle();

        ApplyTextFormatting();
        RefreshTextImage();
    }

    /// @see TextController.as::clone
    // TODO(window-port): Full clone support — callers get null silently
    public virtual TextController? Clone()
    {
        GD.PushWarning("[TextController] Clone() not implemented");
        return null;
    }

    /// @see TextController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        TextStyleManager.StyleChanged -= OnTextStyleChanged;

        if (_localized)
        {
            _context?.RemoveLocalizationListener(ExtractLocalizationKey(_caption), this);
        }

        _margins?.Dispose();
        _margins = null;

        return base.Destroy();
    }

    /// @see TextController.as::set properties (IList variant)
    public virtual void SetProperties(IList<object>? properties)
    {
        if (properties == null)
        {
            return;
        }

        _drawing = true;

        foreach (object prop in properties)
        {
            if (prop is not KeyValuePair<string, object> kvp)
            {
                continue;
            }

            if (_propertySetterTable.TryGetValue(kvp.Key, out Action<TextController, object?>? setter))
            {
                setter(this, kvp.Value);
            }
        }

        _drawing = false;
        RefreshTextImage();
    }

    /// @see TextController.as::set properties (PropertyStruct[] variant)
    public override void ApplyProperties(PropertyStruct[] properties)
    {
        _drawing = true;

        foreach (PropertyStruct prop in properties)
        {
            if (_propertySetterTable.TryGetValue(prop.key, out Action<TextController, object?>? setter))
            {
                setter(this, prop.value);
            }
        }

        _drawing = false;
        RefreshTextImage();

        base.ApplyProperties(properties);
    }

    /// @see TextController.as::setTextFormatting (static)
    private void ApplyTextFormatting()
    {
        TextStyle? style = TextStyleManager.GetStyle(TextStyleName) ?? TextStyleManager.GetStyle("regular");

        if (style == null)
        {
            return;
        }

        // @see TextController.as — merge style with explicit overrides
        FontFace = _explicitStyle.FontFamily ?? style.FontFamily ?? "Ubuntu";
        FontSize = _explicitStyle.FontSize ?? style.FontSize ?? 12;
        textColor = _explicitStyle.Color ?? style.Color ?? 0;
        Bold = (_explicitStyle.FontWeight ?? style.FontWeight ?? "normal") == "bold";
        Italic = (_explicitStyle.FontStyle ?? style.FontStyle ?? "normal") == "italic";
        Underline = (_explicitStyle.TextDecoration ?? style.TextDecoration ?? "none") == "underline";
        Leading = _explicitStyle.Leading ?? style.Leading ?? 0;
        Spacing = _explicitStyle.LetterSpacing ?? style.LetterSpacing ?? 0;
        Kerning = _explicitStyle.Kerning ?? style.Kerning ?? false;
        Sharpness = _explicitStyle.Sharpness ?? style.Sharpness ?? 0;
        Thickness = _explicitStyle.Thickness ?? style.Thickness ?? 0;

        if (_explicitStyle.EtchingColor != null)
        {
            etchingColor = _explicitStyle.EtchingColor.Value;
        }
        else if (style.EtchingColor != null)
        {
            etchingColor = style.EtchingColor.Value;
        }

        if (_explicitStyle.EtchingPosition != null)
        {
            etchingPosition = _explicitStyle.EtchingPosition;
        }
        else if (style.EtchingPosition != null)
        {
            etchingPosition = style.EtchingPosition;
        }
    }

    /// @see TextController.as::onTextStyleChanged
    private void OnTextStyleChanged()
    {
        TextImageRenderer.InvalidateCache(TextStyleName);
        ApplyTextFormatting();
        RefreshTextImage();
    }

    /// @see TextController.as::setTextMargins (callback)
    private void OnSetTextMargins(IMargins? value)
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
                    OnSetTextMargins
                );
            }
            else
            {
                _margins = new TextMargins(
                    value.Left,
                    value.Top,
                    value.Right,
                    value.Bottom,
                    OnSetTextMargins
                );
            }
        }

        RefreshTextImage();
    }

    /// @see TextController.as — extracts key from "${key}" pattern: slice(2, indexOf("}"))
    protected static string ExtractLocalizationKey(string caption)
    {
        int endIndex = caption.IndexOf('}');

        return endIndex > 2 ? caption[2..endIndex] : caption[2..];
    }

    /// @see TextController.as::createPropertySetterTable
    private static Dictionary<string, Action<TextController, object?>> CreatePropertySetterTable()
    {
        return new Dictionary<string, Action<TextController, object?>>
        {
            ["background"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetTextBackground(b);
                }
            },
            ["background_color"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetTextBackgroundColor(Convert.ToUInt32(v));
                }
            },
            ["bold"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetBold(b);
                }
            },
            ["border"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetBorder(b);
                }
            },
            ["border_color"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetBorderColor(Convert.ToUInt32(v));
                }
            },
            ["etching_color"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetEtchingColor(Convert.ToUInt32(v));
                }
            },
            ["etching_position"] = (tc, v) => tc.SetEtchingPosition(v?.ToString()),
            ["font_face"] = (tc, v) => tc.SetFontFace(v?.ToString()),
            ["font_size"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetFontSize(Convert.ToInt32(v));
                }
            },
            ["grid_fit_type"] = (tc, v) => tc.SetGridFitType(v?.ToString()),
            ["italic"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetItalic(b);
                }
            },
            ["kerning"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetKerning(b);
                }
            },
            ["max_chars"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetMaxChars(Convert.ToInt32(v));
                }
            },
            ["multiline"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetMultiline(b);
                }
            },
            ["restrict"] = (tc, v) => tc.SetRestrict(v?.ToString()),
            ["spacing"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetSpacing(Convert.ToInt32(v));
                }
            },
            ["sharpness"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetSharpness(Convert.ToInt32(v));
                }
            },
            ["thickness"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetThickness(Convert.ToInt32(v));
                }
            },
            ["underline"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetUnderline(b);
                }
            },
            ["word_wrap"] = (tc, v) =>
            {
                if (v is bool b)
                {
                    tc.SetWordWrap(b);
                }
            },
            ["margins"] = (tc, v) =>
            {
                if (v is IDictionary<string, object> map)
                {
                    tc.SetTextMarginMap(map);
                }
            },
            ["max_lines"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetMaxLines(Convert.ToInt32(v));
                }
            },
            ["leading"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetLeading(Convert.ToInt32(v));
                }
            },
            ["antialias_type"] = (tc, v) => tc.SetAntiAliasType(v?.ToString()),
            ["auto_size"] = (tc, v) => tc.SetAutoSize(v?.ToString()),
            ["text_color"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetTextColor(Convert.ToUInt32(v));
                }
            },
            ["text_style"] = (tc, v) => tc.SetTextStyleString(v?.ToString()),
            ["margin_left"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetTextMargins(Convert.ToInt32(v), tc._margins?.TopValue ?? 0, tc._margins?.RightValue ?? 0,
                        tc._margins?.BottomValue ?? 0);
                }
            },
            ["margin_top"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetTextMargins(tc._margins?.LeftValue ?? 0, Convert.ToInt32(v), tc._margins?.RightValue ?? 0,
                        tc._margins?.BottomValue ?? 0);
                }
            },
            ["margin_right"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetTextMargins(tc._margins?.LeftValue ?? 0, tc._margins?.TopValue ?? 0, Convert.ToInt32(v),
                        tc._margins?.BottomValue ?? 0);
                }
            },
            ["margin_bottom"] = (tc, v) =>
            {
                if (v != null)
                {
                    tc.SetTextMargins(tc._margins?.LeftValue ?? 0, tc._margins?.TopValue ?? 0, tc._margins?.RightValue ?? 0,
                        Convert.ToInt32(v));
                }
            },
            ["overflow_replace"] = (tc, v) => tc.SetOverflowReplace(v?.ToString()),
        };
    }

	// ITextWindow explicit property implementations

	string ITextWindow.AntiAliasType { get => AntiAliasType; set => SetAntiAliasType(value); }

	string ITextWindow.AutoSize { get => autoSize; set => SetAutoSize(value); }

	bool ITextWindow.Bold { get => Bold; set => SetBold(value); }

	bool ITextWindow.Border { get => Border; set => SetBorder(value); }

	uint ITextWindow.BorderColor { get => BorderColor; set => SetBorderColor(value); }

	int ITextWindow.BottomScrollV => BottomScrollV;

	object? ITextWindow.DefaultTextFormat { get => null; set { } }

	bool ITextWindow.EmbedFonts { get => EmbedFonts; set => SetEmbedFonts(value); }

	string ITextWindow.FontFace { get => FontFace; set => SetFontFace(value); }

	int ITextWindow.FontSize { get => FontSize; set => SetFontSize(value); }

	string ITextWindow.GridFitType { get => GridFitType; set => SetGridFitType(value); }

	string ITextWindow.HtmlText { get => htmlText; set => SetHtmlText(value); }

	bool ITextWindow.Italic { get => Italic; set => SetItalic(value); }

	bool ITextWindow.Kerning { get => Kerning; set => SetKerning(value); }

	IMargins ITextWindow.Margins => TextMargins;

	int ITextWindow.MaxChars { get => MaxChars; set => SetMaxChars(value); }

	bool ITextWindow.Multiline { get => Multiline; set => SetMultiline(value); }

	int ITextWindow.Sharpness { get => Sharpness; set => SetSharpness(value); }

	int ITextWindow.Spacing { get => Spacing; set => SetSpacing(value); }

	string ITextWindow.Text { get => _text; set => SetText(value); }

	uint ITextWindow.TextColor { get => textColor; set => SetTextColor(value); }

	bool ITextWindow.TextBackground { get => TextBackground; set => SetTextBackground(value); }

	uint ITextWindow.TextBackgroundColor { get => TextBackgroundColor; set => SetTextBackgroundColor(value); }

	object? ITextWindow.TextStyle
	{
		get => textStyle;
		set
		{
			switch (value)
			{
				case TextStyle s:
					SetTextStyle(s);
					break;
				case string str:
					SetTextStyleString(str);
					break;
			}
		}
	}

	int ITextWindow.Thickness { get => Thickness; set => SetThickness(value); }

	bool ITextWindow.Underline { get => Underline; set => SetUnderline(value); }

	bool ITextWindow.WordWrap { get => WordWrap; set => SetWordWrap(value); }

	uint ITextWindow.EtchingColor { get => etchingColor; set => SetEtchingColor(value); }

	string? ITextWindow.EtchingPosition { get => etchingPosition; set => SetEtchingPosition(value); }

	object? ITextWindow.StyleSheet { set => SetStyleSheet(value); }

	// ITextWindow explicit method implementations — ported from Flash TextField API delegates

	/// @see TextController.as::getCharBoundaries
	// TODO(window-port): Needs per-glyph bounding box from Font
	object? ITextWindow.GetCharBoundaries(int index)
    {
        return null;
    }

    /// @see TextController.as::getCharIndexAtPoint
	// TODO(window-port): Needs font glyph measurement for char-level hit-testing
	int ITextWindow.GetCharIndexAtPoint(float x, float y)
    {
        return -1;
    }

    /// @see TextController.as::getFirstCharInParagraph
	int ITextWindow.GetFirstCharInParagraph(int charIndex)
	{
		if (charIndex < 0 || charIndex >= _text.Length)
		{
			return -1;
		}

		int pos = _text.LastIndexOf('\n', charIndex);

		return pos < 0 ? 0 : pos + 1;
	}

	/// @see TextController.as::getImageReference — not applicable (no embedded DisplayObjects)
	object? ITextWindow.GetImageReference(string id)
    {
        return null;
    }

    /// @see TextController.as::getLineIndexAtPoint
	int ITextWindow.GetLineIndexAtPoint(float x, float y)
	{
		if (NumLines <= 1)
		{
			return 0;
		}

		TextFieldCache.CachedFontEntry? fontEntry = TextFieldCache.GetByStyleName(TextStyleName);

		if (fontEntry == null)
		{
			return -1;
		}

		float lineH = fontEntry.Font.GetHeight(fontEntry.FontSize) + Leading;

		if (lineH <= 0)
		{
			return -1;
		}

		int line = (int)(y / lineH);

		return Math.Clamp(line, 0, NumLines - 1);
	}

	/// @see TextController.as::getLineIndexOfChar
	int ITextWindow.GetLineIndexOfChar(int charIndex)
	{
		if (charIndex < 0 || charIndex > _text.Length)
		{
			return -1;
		}

		int count = 0;

		for (int i = 0; i < charIndex && i < _text.Length; i++)
		{
			if (_text[i] == '\n')
			{
				count++;
			}
		}

		return count;
	}

	/// @see TextController.as::getLineLength
	int ITextWindow.GetLineLength(int lineIndex)
	{
		string[] lines = _text.Split('\n');

		if (lineIndex < 0 || lineIndex >= lines.Length)
		{
			return 0;
		}

		return lines[lineIndex].Length;
	}

	/// @see TextController.as::getLineMetrics
	// TODO(window-port): Return proper TextLineMetrics struct with ascent/descent/width
	object? ITextWindow.GetLineMetrics(int lineIndex)
    {
        return null;
    }

    /// @see TextController.as::getLineOffset
	int ITextWindow.GetLineOffset(int lineIndex)
	{
		if (lineIndex <= 0)
		{
			return 0;
		}

		int offset = 0;
		int currentLine = 0;

		for (int i = 0; i < _text.Length; i++)
		{
			if (currentLine == lineIndex)
			{
				return offset;
			}

			if (_text[i] == '\n')
			{
				currentLine++;
			}

			offset++;
		}

		return offset;
	}

	/// @see TextController.as::getLineText
	string? ITextWindow.GetLineText(int lineIndex)
	{
		string[] lines = _text.Split('\n');

		if (lineIndex < 0 || lineIndex >= lines.Length)
		{
			return null;
		}

		return lines[lineIndex];
	}

	/// @see TextController.as::getParagraphLength
	int ITextWindow.GetParagraphLength(int charIndex)
	{
		if (charIndex < 0 || charIndex >= _text.Length)
		{
			return 0;
		}

		int first = _text.LastIndexOf('\n', charIndex);
		first = first < 0 ? 0 : first + 1;

		int next = _text.IndexOf('\n', charIndex);
		int end = next < 0 ? _text.Length : next + 1;

		return end - first;
	}

	/// @see TextController.as::getTextFormat — no range-format tracking in Godot port
	object? ITextWindow.GetTextFormat(int beginIndex, int endIndex)
    {
        return null;
    }

    /// @see TextController.as::replaceText
	void ITextWindow.ReplaceText(int beginIndex, int endIndex, string newText)
	{
		if (beginIndex < 0 || endIndex > _text.Length || beginIndex > endIndex)
		{
			return;
		}

		_text = string.Concat(_text.AsSpan(0, beginIndex), newText, _text.AsSpan(endIndex));
		RefreshTextImage();
	}

	/// @see TextController.as::setTextFormat — guard: valid range only
	void ITextWindow.SetTextFormat(object? format, int beginIndex, int endIndex)
	{
		if (beginIndex >= 0 && endIndex > beginIndex && endIndex < _text.Length)
		{
			RefreshTextImage();
		}
	}

    // IScrollableWindow — explicit interface (ScrollH/ScrollV have private set, need explicit bridges)
    float IScrollableWindow.ScrollH { get => ScrollH; set => ScrollH = value; }

    float IScrollableWindow.ScrollV { get => ScrollV; set => ScrollV = value; }
}
