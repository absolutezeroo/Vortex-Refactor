// @see habbo/window/widgets/class_3563.as

using Vortex.Core.Window.Components;

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/class_3563.as
public interface IClass3563
{
    /// @see habbo/window/widgets/class_3563.as::bitmapWrapper
    IStaticBitmapWrapperWindow? BitmapWrapper { get; }

    /// @see habbo/window/widgets/class_3563.as::normalAsset
    string? NormalAsset { get; set; }

    /// @see habbo/window/widgets/class_3563.as::hoverAsset
    string? HoverAsset { get; set; }
}
