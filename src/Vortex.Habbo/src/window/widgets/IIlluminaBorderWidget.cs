// @see habbo/window/widgets/IIlluminaBorderWidget.as

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IIlluminaBorderWidget.as
public interface IIlluminaBorderWidget
{
    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::borderStyle
    string BorderStyle { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::contentChild
    string ContentChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::contentPadding
    uint ContentPadding { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::sidePadding
    uint SidePadding { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::childMargin
    uint ChildMargin { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::topLeftChild
    string TopLeftChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::topCenterChild
    string TopCenterChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::topRightChild
    string TopRightChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::bottomLeftChild
    string BottomLeftChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::bottomCenterChild
    string BottomCenterChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::bottomRightChild
    string BottomRightChild { get; set; }

    /// @see habbo/window/widgets/IIlluminaBorderWidget.as::landingViewMode
    bool LandingViewMode { get; set; }
}
