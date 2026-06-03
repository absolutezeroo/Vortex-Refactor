// @see core/window/components/BackgroundController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Iterators;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/BackgroundController.as
public class BackgroundController : WindowController, IClass3463
{
    /// @see BackgroundController.as::BackgroundController (default)
    public BackgroundController() : base() { }

    /// @see BackgroundController.as::BackgroundController (name + rect)
    public BackgroundController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see BackgroundController.as::BackgroundController (full AS3 11-param signature)
    public BackgroundController
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        IWindowContext param5,
        Rect2 param6,
        IWindow? param7,
        Action<WindowEvent, IWindow>? param8 = null,
        IList<object>? param9 = null,
        IList<string>? param10 = null,
        uint param11 = 0, string param12 = ""
    ) : base(param1, param2, param3, param4, param5, param6, param7, param8, param9, param10, param11, param12)
    {
        // @see BackgroundController.as — force background=true and color=white
        background = true;
        color = 0xFFFFFFFF;
    }

    /// @see BackgroundController.as::get iterator
    public virtual object? Iterator()
    {
        return new ContainerIterator(this);
    }
}
