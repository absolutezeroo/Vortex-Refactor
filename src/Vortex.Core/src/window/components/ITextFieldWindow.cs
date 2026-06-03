// @see core/window/components/ITextFieldWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ITextFieldWindow.as
public interface ITextFieldWindow
{
    /// @see core/window/components/ITextFieldWindow.as::get/set editable
    bool Editable { get; set; }

    /// @see core/window/components/ITextFieldWindow.as::get/set selectable
    bool Selectable { get; set; }

    /// @see core/window/components/ITextFieldWindow.as::get/set displayAsPassword
    bool DisplayAsPassword { get; set; }

    /// @see core/window/components/ITextFieldWindow.as::get/set displayRaw
    bool DisplayRaw { get; set; }

    /// @see core/window/components/ITextFieldWindow.as::get selectionBeginIndex
    int SelectionBeginIndex { get; }

    /// @see core/window/components/ITextFieldWindow.as::get selectionEndIndex
    int SelectionEndIndex { get; }

    /// @see core/window/components/ITextFieldWindow.as::get/set restrict
    string? Restrict { get; set; }

    /// @see core/window/components/ITextFieldWindow.as::setSelection
    void SetSelection(int begin, int end);

    /// @see core/window/components/ITextFieldWindow.as::requestChangeEvent
    void RequestChangeEvent();

    /// @see core/window/components/ITextFieldWindow.as::getWordAt
    string? GetWordAt(int index);
}
