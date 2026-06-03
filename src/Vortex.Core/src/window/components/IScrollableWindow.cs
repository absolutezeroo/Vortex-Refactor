// @see core/window/components/IScrollableWindow.as

using Godot;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IScrollableWindow.as
public interface IScrollableWindow : IWindow
{
    /// @see core/window/components/IScrollableWindow.as::get/set scrollH
    float ScrollH { get; set; }

    /// @see core/window/components/IScrollableWindow.as::get/set scrollV
    float ScrollV { get; set; }

    /// @see core/window/components/IScrollableWindow.as::get/set scrollStepH
    float ScrollStepH { get; set; }

    /// @see core/window/components/IScrollableWindow.as::get/set scrollStepV
    float ScrollStepV { get; set; }

    /// @see core/window/components/IScrollableWindow.as::get maxScrollH
    float MaxScrollH { get; }

    /// @see core/window/components/IScrollableWindow.as::get maxScrollV
    float MaxScrollV { get; }

    /// @see core/window/components/IScrollableWindow.as::get visibleRegion
    Rect2 VisibleRegion { get; }

    /// @see core/window/components/IScrollableWindow.as::get scrollableRegion
    Rect2 ScrollableRegion { get; }
}
