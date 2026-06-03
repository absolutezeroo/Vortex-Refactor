// @see core/window/components/TextFieldController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/TextFieldController.as
/// Interactive text field supporting editable input, focus, keyboard events.
public class TextFieldController : TextController, ITextFieldWindow, IInteractiveWindow
{
    private readonly bool _focusCapturer;
    private readonly uint _etchingFilterColor;
    private readonly float _etchingFilterAlpha;

    // @see TextFieldController.as — interactive properties
    private string _mouseCursorType = "";
    private string _toolTipCaption = "";
    private readonly Dictionary<uint, uint> _cursorByState = new();

    /// @see TextFieldController.as::TextFieldController (default)
    public TextFieldController() : base() { }

    /// @see TextFieldController.as::TextFieldController (name + rect)
    public TextFieldController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see TextFieldController.as::TextFieldController (full AS3 11-param signature)
    public TextFieldController
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
    ) : base(param1, param2, param3, (uint)((param4 & ~16u) | 1), param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see TextFieldController.as — create GC_TYPE_TEXTFIELD graphic context
        _var1650 ??= new GraphicContext(
                name, GraphicContext.GC_TYPE_TEXTFIELD,
                new Rect2(base.x, base.y, base.width, base.height)
            )
        {
            Visible = _visible,
        };
    }

    /// @see TextFieldController.as::getGraphicContext
    public override IGraphicContext? GetGraphicContext(bool create)
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
            name, GraphicContext.GC_TYPE_TEXTFIELD,
            new Rect2(base.x, base.y, base.width, base.height)
        )
        {
            Visible = _visible,
        };

        return _var1650;
    }

    /// @see TextFieldController.as::get editable
    public bool Editable { get; set; }

    /// @see TextFieldController.as::get selectable
    public bool Selectable { get; set; }

    /// @see TextFieldController.as::get displayAsPassword
    public bool DisplayAsPassword { get; set; }

    /// @see TextFieldController.as::get displayRaw
    public bool DisplayRaw { get; set; }

    /// @see TextFieldController.as::get focused
    public bool Focused { get; private set; }

    /// @see TextFieldController.as::get selectionBeginIndex
    public int SelectionBeginIndex { get; private set; }

    /// @see TextFieldController.as::get selectionEndIndex
    public int SelectionEndIndex { get; private set; }

    /// @see TextFieldController.as::get mouseCursorType
    public string MouseCursorType
    {
        get => _mouseCursorType;
        set => _mouseCursorType = value ?? "";
    }

    /// @see TextFieldController.as::get toolTipCaption
    public string ToolTipCaption
    {
        get => _toolTipCaption;
        set => _toolTipCaption = value ?? "";
    }

    /// @see TextFieldController.as::get toolTipDelay
    public uint ToolTipDelay { get; set; } = 500;

    /// @see TextFieldController.as::get toolTipIsDynamic
    public bool ToolTipIsDynamic { get; set; }

    /// @see TextFieldController.as::get interactiveCursorDisabled
    public bool InteractiveCursorDisabled { get; set; }

    /// @see TextFieldController.as::focus
    public new void Focus()
    {
        if (Focused)
        {
            return;
        }

        Focused = true;
        _context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.STATE);
    }

    /// @see TextFieldController.as::unfocus
    public new void Unfocus()
    {
        if (!Focused)
        {
            return;
        }

        Focused = false;
        _context?.Invalidate(this, new Rect2(0, 0, width, height), Class3655.STATE);
    }

    /// @see TextFieldController.as::enable
    public new void Enable()
    {
        Editable = true;
    }

    /// @see TextFieldController.as::disable
    public new void Disable()
    {
        Editable = false;
    }

    /// @see TextFieldController.as::setSelection
    public virtual void SetSelection(int beginIndex, int endIndex)
    {
        SelectionBeginIndex = beginIndex;
        SelectionEndIndex = endIndex;
    }

    /// @see TextFieldController.as::setMouseCursorForState
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

    /// @see TextFieldController.as::getMouseCursorByState
    public uint GetMouseCursorByState(uint state)
    {
        return _cursorByState.GetValueOrDefault(state, 0u);
    }

    /// @see TextFieldController.as::showToolTip / hideToolTip
    public virtual void ShowToolTip()
    {
        /* Deferred: tooltip system */
    }

    public virtual void HideToolTip()
    {
        /* Deferred: tooltip system */
    }

    /// @see TextFieldController.as::update
    public override bool Update(WindowController param1, WindowEvent param2)
    {
        switch (param2.type)
        {
            case WindowEvent.WE_ACTIVATED:
                Focus();
                break;
            case WindowEvent.WE_RESIZED:
                // @see TextFieldController.as — sync dimensions on resize
                RefreshTextImage(true);
                break;
        }

        return base.Update(param1, param2);
    }

    /// @see TextFieldController.as::refreshTextImage
    protected override void RefreshTextImage(bool suppressResizeEvent = false)
    {
        // @see TextFieldController.as — update etching filters then delegate
        base.RefreshTextImage(suppressResizeEvent);
    }

    /// @see TextFieldController.as::set text
    public override void SetText(string? value)
    {
        base.SetText(value);
        RefreshAutoSize();
    }

    /// @see TextFieldController.as::refreshAutoSize
    protected virtual void RefreshAutoSize()
    {
        if (autoSize == "none")
        {
            return;
        }

        // @see TextFieldController.as — resize window to fit text content
        float marginH = (TextMargins as Utils.TextMargins)?.LeftValue + (TextMargins as Utils.TextMargins)?.RightValue ?? 0;
        float marginV = (TextMargins as Utils.TextMargins)?.TopValue + (TextMargins as Utils.TextMargins)?.BottomValue ?? 0;

        float newW = TextWidth + marginH;
        float newH = TextHeight + marginV;

        if (!(Math.Abs(width - newW) > 0.5f) && !(Math.Abs(height - newH) > 0.5f))
        {
            return;
        }

        base.width = Math.Max(newW, 1);
        base.height = Math.Max(newH, 1);
    }

    /// @see TextFieldController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        if (Focused)
        {
            Unfocus();
        }

        _cursorByState.Clear();

        return base.Destroy();
    }

    /// @see TextFieldController.as::requestChangeEvent
    /// Programmatically fires a WE_CHANGE event (same as native text field change).
    public virtual void RequestChangeEvent()
    {
        RefreshAutoSize();

        WindowEvent evt = WindowEvent.Allocate(WindowEvent.WE_CHANGE, this, null);
        Update(this, evt);
    }

    /// @see TextFieldController.as::getWordAt
    /// Returns the word at the given character index, using word boundary delimiters.
    public virtual string? GetWordAt(int charIndex)
    {
        string fieldText = text;

        if (string.IsNullOrEmpty(fieldText) || charIndex < 0 || charIndex >= fieldText.Length)
        {
            return null;
        }

        List<int> wordPositions = GetWordPositions(fieldText);

        for (int i = 0; i < wordPositions.Count; i++)
        {
            int wordStart = wordPositions[i];
            int wordEnd = i + 1 < wordPositions.Count
                ? wordPositions[i + 1] - 1
                : fieldText.Length;

            if (charIndex >= wordStart && charIndex <= wordEnd)
            {
                return fieldText[wordStart..wordEnd];
            }
        }

        return "";
    }

    /// @see TextFieldController.as::getWordPositions
    /// Returns the start indices of each word, splitting on punctuation/whitespace delimiters.
    private static List<int> GetWordPositions(string text)
    {
        List<int> positions = [];
        bool inWord = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            bool isDelimiter = c is '~' or '%' or '&' or '!' or '\\' or ';' or ':'
                or '"' or '\'' or ',' or '<' or '>' or '?' or '#' or '.' or '-'
                or '(' or ')' or '=' or '[' or ']' or '{' or '}' or '^' or '_'
                || char.IsWhiteSpace(c);

            if (!isDelimiter && !inWord)
            {
                positions.Add(i);
                inWord = true;
            }
            else if (isDelimiter)
            {
                inWord = false;
            }
        }

        return positions;
    }

    // ITextFieldWindow explicit property implementations

    /// @see TextFieldController.as — Restrict bridges to SetRestrict
    string? ITextFieldWindow.Restrict
    {
        get => Restrict;
        set => SetRestrict(value);
    }

    // IInteractiveWindow explicit property implementations (public members have matching names)

    /// @see TextFieldController.as — ToolTipCaption bridges to public property
    string? IInteractiveWindow.ToolTipCaption
    {
        get => ToolTipCaption;
        set => ToolTipCaption = value ?? "";
    }

    /// @see TextFieldController.as — ToolTipDelay bridges to public property
    uint IInteractiveWindow.ToolTipDelay
    {
        get => ToolTipDelay;
        set => ToolTipDelay = value;
    }

    /// @see TextFieldController.as — ToolTipIsDynamic bridges to public property
    bool IInteractiveWindow.ToolTipIsDynamic
    {
        get => ToolTipIsDynamic;
        set => ToolTipIsDynamic = value;
    }

    /// @see TextFieldController.as — InteractiveCursorDisabled bridges to public property
    bool IInteractiveWindow.InteractiveCursorDisabled
    {
        get => InteractiveCursorDisabled;
        set => InteractiveCursorDisabled = value;
    }
}
