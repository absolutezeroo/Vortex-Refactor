// @see core/window/components/class_3478.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/class_3478.as
public interface IDropListItemWindow : IButtonWindow
{
    /// @see core/window/components/class_3478.as::get menu
    IDropMenuWindow? Menu();

    /// @see core/window/components/class_3478.as::get value
    IWindow? Value();

    /// @see core/window/components/class_3478.as::set value
    void Value(IWindow? value);
}
