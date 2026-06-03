// @see WIN63-202407091256-704579380-Source-main/core/window/components/class_3460.as

using Godot;

namespace Vortex.Core.Window.Components;

/// @see WIN63-202407091256-704579380-Source-main/core/window/components/class_3460.as
public interface IDesktopWindow : IWindowContainer, IDisplayObjectWrapper
{
    /// @see class_3460.as::get mouseX
    int mouseX { get; }

    /// @see class_3460.as::get mouseY
    int mouseY { get; }

    /// @see class_3460.as::getActiveWindow
    IWindow? GetActiveWindow();

    /// @see class_3460.as::setActiveWindow
    IWindow? SetActiveWindow(IWindow param1);

    /// @see class_3460.as::groupParameterFilteredChildrenUnderPoint
    void GroupParameterFilteredChildrenUnderPoint(Vector2 param1, IList<IWindow> param2, uint param3 = 0);
}
