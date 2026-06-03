// @see habbo/window/widgets/IRunningNumberWidget.as

namespace Vortex.Habbo.Window.Widgets;

/// @see habbo/window/widgets/IRunningNumberWidget.as
public interface IRunningNumberWidget
{
    /// @see habbo/window/widgets/IRunningNumberWidget.as::number
    int Number { get; set; }

    /// @see habbo/window/widgets/IRunningNumberWidget.as::digits
    uint Digits { get; set; }

    /// @see habbo/window/widgets/IRunningNumberWidget.as::colorStyle
    int ColorStyle { get; set; }

    /// @see habbo/window/widgets/IRunningNumberWidget.as::updateFrequency
    int UpdateFrequency { get; set; }

    /// @see habbo/window/widgets/IRunningNumberWidget.as::initialNumber
    int InitialNumber { set; }
}
