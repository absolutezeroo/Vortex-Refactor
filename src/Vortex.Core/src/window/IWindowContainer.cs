// @see WIN63-202407091256-704579380-Source-main/core/window/IWindowContainer.as

using Godot;

using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/IWindowContainer.as
public interface IWindowContainer : IWindow, IIterable
{
    /// @see IWindowContainer.as::getChildUnderPoint
    IWindow? GetChildUnderPoint(Vector2 param1);

    /// @see IWindowContainer.as::groupChildrenUnderPoint
    void GroupChildrenUnderPoint(Vector2 param1, IList<IWindow> param2);
}
