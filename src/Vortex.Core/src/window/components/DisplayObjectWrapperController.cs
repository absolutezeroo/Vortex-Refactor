// @see core/window/components/DisplayObjectWrapperController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/DisplayObjectWrapperController.as
public class DisplayObjectWrapperController : WindowController
{
    /// @see DisplayObjectWrapperController.as::DisplayObjectWrapperController (default)
    public DisplayObjectWrapperController() : base() { }

    /// @see DisplayObjectWrapperController.as::DisplayObjectWrapperController (name + rect)
    public DisplayObjectWrapperController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see DisplayObjectWrapperController.as::DisplayObjectWrapperController (full AS3 11-param signature)
    /// @see DisplayObjectWrapperController.as — param4 &= ~16 before super call
    public DisplayObjectWrapperController
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
    ) : base(param1, param2, param3, param4 & ~16u, param5, param6, param7, param8, param9, param10, param11, param12)
    {
    }

    /// @see DisplayObjectWrapperController.as::getGraphicContext
    /// Godot adaptation: creates GC_TYPE_CONTAINER on demand
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

        _var1650 = new GraphicContext(
            $"GC {{{name}}}",
            GraphicContext.GC_TYPE_CONTAINER,
            new Rect2(base.x, base.y, base.width, base.height)
        )
        {
            Visible = _visible,
        };

        return _var1650;
    }

    /// @see DisplayObjectWrapperController.as::getDisplayObject
    /// Godot adaptation: returns the GC's DisplayNode (Node2D)
    public Node2D? GetDisplayObject()
    {
        return GetGraphicContext(true)?.DisplayNode;
    }

    /// @see DisplayObjectWrapperController.as::setDisplayObject
    /// Godot adaptation: AS3 setDisplayObject → attach a Node2D child to the GC display node
    public void SetDisplayObject(Node2D? param1)
    {
        IGraphicContext? gc = GetGraphicContext(true);

        if (gc == null)
        {
            return;
        }

        // Remove existing children from the display node
        Node2D? displayNode = gc.DisplayNode;

        if (displayNode == null)
        {
            return;
        }

        foreach (Node? child in displayNode.GetChildren())
        {
            displayNode.RemoveChild(child);
        }

        if (param1 != null)
        {
            displayNode.AddChild(param1);
        }
    }
}
