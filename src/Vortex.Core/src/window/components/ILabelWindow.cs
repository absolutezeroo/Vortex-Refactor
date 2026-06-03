// @see core/window/components/ILabelWindow.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ILabelWindow.as
public interface ILabelWindow : IWindow
{
    /// @see core/window/components/ILabelWindow.as::get antiAliasType
    string AntiAliasType { get; }

    /// @see core/window/components/ILabelWindow.as::get autoSize
    string AutoSize { get; }

    /// @see core/window/components/ILabelWindow.as::get bold
    bool Bold { get; }

    /// @see core/window/components/ILabelWindow.as::get border
    bool Border { get; }

    /// @see core/window/components/ILabelWindow.as::get borderColor
    uint BorderColor { get; }

    /// @see core/window/components/ILabelWindow.as::get defaultTextFormat
    object? DefaultTextFormat { get; }

    /// @see core/window/components/ILabelWindow.as::get embedFonts
    bool EmbedFonts { get; }

    /// @see core/window/components/ILabelWindow.as::get fontFace
    string FontFace { get; }

    /// @see core/window/components/ILabelWindow.as::get fontSize
    int FontSize { get; }

    /// @see core/window/components/ILabelWindow.as::get gridFitType
    string GridFitType { get; }

    /// @see core/window/components/ILabelWindow.as::get italic
    bool Italic { get; }

    /// @see core/window/components/ILabelWindow.as::get kerning
    bool Kerning { get; }

    /// @see core/window/components/ILabelWindow.as::get length
    int Length { get; }

    /// @see core/window/components/ILabelWindow.as::get margins
    IMargins? Margins { get; }

    /// @see core/window/components/ILabelWindow.as::get maxChars
    int MaxChars { get; }

    /// @see core/window/components/ILabelWindow.as::get sharpness
    int Sharpness { get; }

    /// @see core/window/components/ILabelWindow.as::get spacing
    int Spacing { get; }

    /// @see core/window/components/ILabelWindow.as::get/set text
    string Text { get; set; }

    /// @see core/window/components/ILabelWindow.as::get/set textColor
    uint TextColor { get; set; }

    /// @see core/window/components/ILabelWindow.as::get textBackground
    bool TextBackground { get; }

    /// @see core/window/components/ILabelWindow.as::get textBackgroundColor
    uint TextBackgroundColor { get; }

    /// @see core/window/components/ILabelWindow.as::get textHeight
    float TextHeight { get; }

    /// @see core/window/components/ILabelWindow.as::get textWidth
    float TextWidth { get; }

    /// @see core/window/components/ILabelWindow.as::get/set textStyle
    object? TextStyle { get; set; }

    /// @see core/window/components/ILabelWindow.as::get thickness
    int Thickness { get; }

    /// @see core/window/components/ILabelWindow.as::get underline
    bool Underline { get; }

    /// @see core/window/components/ILabelWindow.as::get/set vertical
    bool Vertical { get; set; }
}
