// @see core/window/components/ITextWindow.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ITextWindow.as
public interface ITextWindow : IWindow, IScrollableWindow
{
    /// @see core/window/components/ITextWindow.as::get/set antiAliasType
    string AntiAliasType { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set autoSize
    string AutoSize { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set bold
    bool Bold { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set border
    bool Border { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set borderColor
    uint BorderColor { get; set; }

    /// @see core/window/components/ITextWindow.as::get bottomScrollV
    int BottomScrollV { get; }

    /// @see core/window/components/ITextWindow.as::get/set defaultTextFormat
    object? DefaultTextFormat { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set embedFonts
    bool EmbedFonts { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set fontFace
    string FontFace { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set fontSize
    int FontSize { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set gridFitType
    string GridFitType { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set htmlText
    string HtmlText { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set italic
    bool Italic { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set kerning
    bool Kerning { get; set; }

    /// @see core/window/components/ITextWindow.as::get length
    int Length { get; }

    /// @see core/window/components/ITextWindow.as::get margins
    IMargins Margins { get; }

    /// @see core/window/components/ITextWindow.as::get/set maxChars
    int MaxChars { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set multiline
    bool Multiline { get; set; }

    /// @see core/window/components/ITextWindow.as::get numLines
    int NumLines { get; }

    /// @see core/window/components/ITextWindow.as::get/set sharpness
    int Sharpness { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set spacing
    new int Spacing { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set text
    string Text { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set textColor
    uint TextColor { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set textBackground
    bool TextBackground { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set textBackgroundColor
    uint TextBackgroundColor { get; set; }

    /// @see core/window/components/ITextWindow.as::get textHeight
    float TextHeight { get; }

    /// @see core/window/components/ITextWindow.as::get textWidth
    float TextWidth { get; }

    /// @see core/window/components/ITextWindow.as::get/set textStyle
    object? TextStyle { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set thickness
    int Thickness { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set underline
    bool Underline { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set wordWrap
    bool WordWrap { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set etchingColor
    uint EtchingColor { get; set; }

    /// @see core/window/components/ITextWindow.as::get/set etchingPosition
    string? EtchingPosition { get; set; }

    /// @see core/window/components/ITextWindow.as::get isOverflown
    bool IsOverflown { get; }

    /// @see core/window/components/ITextWindow.as::set styleSheet
    object? StyleSheet { set; }

    /// @see core/window/components/ITextWindow.as::appendText
    void AppendText(string? value);

    /// @see core/window/components/ITextWindow.as::getCharBoundaries
    object? GetCharBoundaries(int index);

    /// @see core/window/components/ITextWindow.as::getCharIndexAtPoint
    int GetCharIndexAtPoint(float x, float y);

    /// @see core/window/components/ITextWindow.as::getFirstCharInParagraph
    int GetFirstCharInParagraph(int charIndex);

    /// @see core/window/components/ITextWindow.as::getImageReference
    object? GetImageReference(string id);

    /// @see core/window/components/ITextWindow.as::getLineIndexAtPoint
    int GetLineIndexAtPoint(float x, float y);

    /// @see core/window/components/ITextWindow.as::getLineIndexOfChar
    int GetLineIndexOfChar(int charIndex);

    /// @see core/window/components/ITextWindow.as::getLineLength
    int GetLineLength(int lineIndex);

    /// @see core/window/components/ITextWindow.as::getLineMetrics
    object? GetLineMetrics(int lineIndex);

    /// @see core/window/components/ITextWindow.as::getLineOffset
    int GetLineOffset(int lineIndex);

    /// @see core/window/components/ITextWindow.as::getLineText
    string? GetLineText(int lineIndex);

    /// @see core/window/components/ITextWindow.as::getParagraphLength
    int GetParagraphLength(int charIndex);

    /// @see core/window/components/ITextWindow.as::getTextFormat
    object? GetTextFormat(int beginIndex = -1, int endIndex = -1);

    /// @see core/window/components/ITextWindow.as::replaceText
    void ReplaceText(int beginIndex, int endIndex, string newText);

    /// @see core/window/components/ITextWindow.as::setTextFormat
    void SetTextFormat(object? format, int beginIndex = -1, int endIndex = -1);

    /// @see core/window/components/ITextWindow.as::resetExplicitStyle
    void ResetExplicitStyle();
}
