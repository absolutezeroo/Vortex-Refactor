// @see habbo/window/widgets/IIlluminaInputWidget.as

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IIlluminaInputWidget.as
public interface IIlluminaInputWidget
{
    /// @see habbo/window/widgets/IIlluminaInputWidget.as::message
    string? Message { get; set; }

    /// @see habbo/window/widgets/IIlluminaInputWidget.as::submitHandler
    IIlluminaInputHandler? SubmitHandler { get; set; }

    /// @see habbo/window/widgets/IIlluminaInputWidget.as::buttonCaption
    string? ButtonCaption { get; set; }

    /// @see habbo/window/widgets/IIlluminaInputWidget.as::emptyMessage
    string? EmptyMessage { get; set; }

    /// @see habbo/window/widgets/IIlluminaInputWidget.as::multiline
    bool Multiline { get; set; }

    /// @see habbo/window/widgets/IIlluminaInputWidget.as::maxChars
    int MaxChars { get; set; }
}
