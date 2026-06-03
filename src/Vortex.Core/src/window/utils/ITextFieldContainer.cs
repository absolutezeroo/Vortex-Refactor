// @see core/window/utils/ITextFieldContainer.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/ITextFieldContainer.as
public interface ITextFieldContainer
{
    /// @see core/window/utils/ITextFieldContainer.as::get textField
    object? TextField { get; }

    /// @see core/window/utils/ITextFieldContainer.as::get margins
    IMargins? Margins { get; }
}
