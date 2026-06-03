// @see WIN63-202407091256-704579380-Source-main/core/window/components/class_3431.as

using Godot;

namespace Vortex.Core.Window.Components;

/// @see WIN63-202407091256-704579380-Source-main/core/window/components/class_3431.as
public interface IDisplayObjectWrapper : IWindow
{
    /// @see class_3431.as::setDisplayObject
    /// Godot adaptation: AS3 DisplayObject → Node2D
    void SetDisplayObject(Node2D? param1);

    /// @see class_3431.as::getDisplayObject
    /// Godot adaptation: AS3 DisplayObject → Node2D
    Node2D? GetDisplayObject();
}
