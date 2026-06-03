// @see core/window/components/class_3520.as

namespace Vortex.Core.Window.Components;

/// @see core/window/components/class_3520.as
public interface IDropMenuWindow : IInteractiveWindow
{
    /// @see core/window/components/class_3520.as::get/set selection
    int Selection { get; set; }

    /// @see core/window/components/class_3520.as::get numMenuItems
    int NumMenuItems { get; }

    /// @see core/window/components/class_3520.as::populate
    void Populate(string[] items);

    /// @see core/window/components/class_3520.as::populateWithVector
    void PopulateWithVector(IList<string> items);

    /// @see core/window/components/class_3520.as::enumerateSelection
    List<string> EnumerateSelection();

    /// @see core/window/components/class_3520.as::openMenu
    void OpenMenu();
}
