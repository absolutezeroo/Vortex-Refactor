// @see habbo/window/widgets/class_3654.as

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/class_3654.as
public interface IClass3654
{
    /// @see habbo/window/widgets/class_3654.as::figure
    string? Figure { get; set; }

    /// @see habbo/window/widgets/class_3654.as::scale
    int Scale { get; set; }

    /// @see habbo/window/widgets/class_3654.as::direction
    int Direction { get; set; }

    /// @see habbo/window/widgets/class_3654.as::zoomX
    double ZoomX { get; set; }

    /// @see habbo/window/widgets/class_3654.as::zoomY
    double ZoomY { get; set; }

    /// @see habbo/window/widgets/class_3654.as::petWidth
    int PetWidth { get; }

    /// @see habbo/window/widgets/class_3654.as::petHeight
    int PetHeight { get; }

    /// @see habbo/window/widgets/class_3654.as::shrinkOnOverflow
    bool ShrinkOnOverflow { get; set; }
}
