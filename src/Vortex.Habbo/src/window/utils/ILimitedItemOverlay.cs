// @see habbo/window/utils/ILimitedItemOverlay.as

using Vortex.Core.Window.Components;

namespace Vortex.Habbo.Window.Utils;

/// @see habbo/window/utils/ILimitedItemOverlay.as
public interface ILimitedItemOverlay
{
    /// @see habbo/window/utils/ILimitedItemOverlay.as::get window
    IWidgetWindow? Window { get; }

    /// @see habbo/window/utils/ILimitedItemOverlay.as::set serialNumber
    int SerialNumber { set; }

    /// @see habbo/window/utils/ILimitedItemOverlay.as::set seriesSize
    int SeriesSize { set; }
}
