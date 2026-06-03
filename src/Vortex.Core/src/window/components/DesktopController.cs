// @see WIN63-202407091256-704579380-Source-main/core/window/components/DesktopController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics;

namespace Vortex.Core.Window.Components;

/// @see WIN63-202407091256-704579380-Source-main/core/window/components/DesktopController.as
public class DesktopController : ActivatorController, IDesktopWindow, IDisplayObjectWrapper
{
    private Node2D? _displayObjectRef;

    /// @see DesktopController.as::DesktopController
    public DesktopController(string param1, IWindowContext param2, Rect2 param3)
        : base(param1, 0, 0, 0, param2, param3, null, DefaultProcedure, null, null, 0)
    {
    }

    /// @see DesktopController.as::get mouseX
    /// Godot adaptation: AS3 uses stage.mouseX, adapted to viewport mouse position.
    public int mouseX => (int)GetViewportMousePosition().X;

    /// @see DesktopController.as::get mouseY
    /// Godot adaptation: AS3 uses stage.mouseY, adapted to viewport mouse position.
    public int mouseY => (int)GetViewportMousePosition().Y;

    /// Godot adaptation: AS3 reads stage.mouseX/mouseY. In Godot we query the
    /// viewport's mouse position via DisplayServer, which returns screen-relative
    /// coords already usable as "desktop-space" coordinates.
    private static Vector2 GetViewportMousePosition()
    {
        Godot.Window? viewport = (Engine.GetMainLoop() as SceneTree)?.Root;

        if (viewport != null)
        {
            return viewport.GetMousePosition();
        }

        return Vector2.Zero;
    }

    /// @see DesktopController.as::set parent — desktop has no parent
    public static void SetParent(IWindow? value)
    {
        throw new Exception("Desktop window doesn't have parent!");
    }

    /// @see DesktopController.as::set procedure — fallback to DefaultProcedure if null
    public override Action<WindowEvent, IWindow>? procedure
    {
        get => _procedure;
        set => _procedure = value ?? DefaultProcedure;
    }

    /// @see DesktopController.as::getActiveWindow
    public IWindow? GetActiveWindow()
    {
        return GetActiveChild();
    }

    /// @see DesktopController.as::setActiveWindow
    public IWindow? SetActiveWindow(IWindow param1)
    {
        return SetActiveChild(param1);
    }

    /// @see DesktopController.as::getGraphicContext
    /// Godot adaptation: creates GC_TYPE_EMPTY for desktop (container-only, no bitmap).
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
            name, GraphicContext.GC_TYPE_EMPTY,
            new Rect2(base.x, base.y, base.width, base.height)
        )
        {
            Mouse = true,
        };

        return _var1650;
    }

    /// @see DesktopController.as::getDisplayObject
    /// Godot adaptation: returns the GraphicContext's display node.
    public Node2D? GetDisplayObject()
    {
        return GetGraphicContext(true)?.DisplayNode;
    }

    /// @see DesktopController.as::setDisplayObject
    /// Godot adaptation: stores reference (AS3 desktop is no-op for set).
    public void SetDisplayObject(Node2D? param1)
    {
        _displayObjectRef = param1;
    }

    /// @see DesktopController.as::invalidate — no-op for desktop
    public override void Invalidate(Rect2? param1 = null) { }

    /// @see DesktopController.as::groupParameterFilteredChildrenUnderPoint
    /// Desktop override: flat iteration only (no recursion into children at root level).
    public override void GroupParameterFilteredChildrenUnderPoint(
        Vector2 param1, IList<IWindow> param2, uint param3 = 0)
    {
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            IWindow child = _children[i];

            if (!child.visible)
            {
                continue;
            }

            if (param3 != 0 && (child.param & param3) == 0)
            {
                continue;
            }

            if (param1.X >= child.x && param1.X <= child.x + child.width &&
                param1.Y >= child.y && param1.Y <= child.y + child.height)
            {
                param2.Add(child);
            }
        }
    }

    /// @see DesktopController.as::defaultProcedure
    private static void DefaultProcedure(WindowEvent param1, IWindow param2) { }
}
