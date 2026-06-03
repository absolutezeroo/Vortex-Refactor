// @see core/window/components/IScrollbarWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IScrollbarWindow.as
public interface IScrollbarWindow
{
    /// @see core/window/components/IScrollbarWindow.as::get/set scrollH
    float ScrollH { get; set; }

    /// @see core/window/components/IScrollbarWindow.as::get/set scrollV
    float ScrollV { get; set; }

    /// @see core/window/components/IScrollbarWindow.as::get/set scrollable
    IScrollableWindow? Scrollable { get; set; }

    /// @see core/window/components/IScrollbarWindow.as::get vertical
    bool Vertical { get; }

    /// @see core/window/components/IScrollbarWindow.as::get horizontal
    bool Horizontal { get; }
}
