// @see core/window/components/class_3514.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/class_3514.as
public interface IClass3514
{
    /// @see core/window/components/class_3514.as::get title
    IWindow? Title { get; }

    /// @see core/window/components/class_3514.as::get header
    IWindow? Header { get; }

    /// @see core/window/components/class_3514.as::get content
    IWindow? Content { get; }

    /// @see core/window/components/class_3514.as::get margins
    IMargins? Margins { get; }

    /// @see core/window/components/class_3514.as::get scaler
    IWindow? Scaler { get; }

    /// @see core/window/components/class_3514.as::get helpPage
    string HelpPage { get; set; }

    /// @see core/window/components/class_3514.as::set helpButtonAction
    object? HelpButtonAction { set; }

    /// @see core/window/components/class_3514.as::resizeToFitContent
    void ResizeToFitContent();
}
