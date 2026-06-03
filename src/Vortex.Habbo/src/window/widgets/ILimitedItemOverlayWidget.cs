// @see habbo/window/widgets/ILimitedItemOverlayWidget.as

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/ILimitedItemOverlayWidget.as
public interface ILimitedItemOverlayWidget
{
    /// @see habbo/window/widgets/ILimitedItemOverlayWidget.as::serialNumber
    int SerialNumber { get; set; }

    /// @see habbo/window/widgets/ILimitedItemOverlayWidget.as::seriesSize
    int SeriesSize { get; set; }
}
