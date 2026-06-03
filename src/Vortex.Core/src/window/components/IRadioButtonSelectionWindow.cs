// @see core/window/components/IRadioButtonSelectionWindow.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IRadioButtonSelectionWindow.as
public interface IRadioButtonSelectionWindow
{
    /// @see core/window/components/IRadioButtonSelectionWindow.as::get selected
    IWindow? Selected { get; }

    /// @see core/window/components/IRadioButtonSelectionWindow.as::radioButtonSelection
    void RadioButtonSelection(IWindow button);
}
