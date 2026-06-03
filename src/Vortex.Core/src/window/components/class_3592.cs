// @see core/window/components/class_3592.as

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/class_3592.as
public interface IClass3592
{
    /// @see core/window/components/class_3592.as::get direction
    string Direction { get; set; }

    /// @see core/window/components/class_3592.as::get pointerOffset
    int PointerOffset { get; set; }

    /// @see core/window/components/class_3592.as::get margins
    IMargins? Margins { get; }

    /// @see core/window/components/class_3592.as::get content
    IWindow? Content { get; }
}
