// @see core/window/components/ContainerController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;
using Vortex.Core.Window.Iterators;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/ContainerController.as
public class ContainerController : WindowController, IWindowContainer
{
    /// @see ContainerController.as::ContainerController (default)
    public ContainerController() : base() { }

    /// @see ContainerController.as::ContainerController (name + rect)
    public ContainerController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see ContainerController.as::ContainerController (full AS3 11-param signature)
    public ContainerController
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
    }

    /// @see ContainerController.as::getGraphicContext
    /// Godot adaptation: flag 16 → container-only GC; otherwise bitmap GC.
    public override IGraphicContext? GetGraphicContext(bool create)
    {
        if (_var1650 != null)
        {
            return _var1650;
        }

        if (!create)
        {
            return null;
        }

        uint gcType = TestParamFlag(16)
            ? GraphicContext.GC_TYPE_CONTAINER
            : GraphicContext.GC_TYPE_BITMAP;

        _var1650 = new GraphicContext(
            name, gcType,
            new Rect2(base.x, base.y, base.width, base.height)
        )
        {
            Visible = _visible,
        };

        return _var1650;
    }

    /// @see ContainerController.as::get iterator
    public virtual object? Iterator()
    {
        return new ContainerIterator(this);
    }
}
