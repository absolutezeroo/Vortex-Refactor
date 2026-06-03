// @see core/window/utils/class_3348.as

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3348.as
public interface IClass3348
{
    /// @see core/window/utils/class_3348.as::get/set title
    string? Title { get; set; }

    /// @see core/window/utils/class_3348.as::get/set summary
    string? Summary { get; set; }

    /// @see core/window/utils/class_3348.as::get/set callback
    object? Callback { get; set; }

    /// @see core/window/utils/class_3348.as::get/set titleBarColor
    uint TitleBarColor { get; set; }

    /// @see core/window/utils/class_3348.as::getButtonCaption
    IClass3562? GetButtonCaption(int buttonFlag);

    /// @see core/window/utils/class_3348.as::setButtonCaption
    void SetButtonCaption(int buttonFlag, IClass3562? caption);
}
