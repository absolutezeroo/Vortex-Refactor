// @see core/window/components/WidgetWindowController.as

using System;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/WidgetWindowController.as
public class WidgetWindowController : WindowController, IWidgetWindow
{
    private IWidgetFactory? _widgetFactory;
    private readonly string _widgetType = "";

    /// @see WidgetWindowController.as::WidgetWindowController (default)
    public WidgetWindowController() : base() { }

    /// @see WidgetWindowController.as::WidgetWindowController (name + rect)
    public WidgetWindowController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see WidgetWindowController.as::WidgetWindowController (full AS3 11-param signature)
    public WidgetWindowController
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
        // @see WidgetWindowController.as — get widget factory from context before super call
        _widgetFactory = param5.GetWidgetFactory();
    }

    /// @see WidgetWindowController.as::dispose
    public override bool Destroy()
    {
        if (_disposed)
        {
            return false;
        }

        if (WidgetInstance != null)
        {
            WidgetInstance.Dispose();
            WidgetInstance = null;
        }

        _widgetFactory = null;

        return base.Destroy();
    }

    /// @see WidgetWindowController.as::set color
    public new uint color
    {
        get => base.color;
        set
        {
            base.color = value;

            List<IWindow> colorizeChildren = new();
            GroupChildrenWithTag(TAG_COLORIZE, colorizeChildren, -1);

            foreach (IWindow child in colorizeChildren)
            {
                child.color = value;
            }
        }
    }

    /// @see WidgetWindowController.as::get iterator
    public virtual object? Iterator()
    {
        return WidgetInstance != null ? null : null;
    }

    /// @see WidgetWindowController.as::get widget
    public IWidget? WidgetInstance { get; private set; }

    /// @see IWidgetWindow — explicit interface
    IWidget? IWidgetWindow.Widget()
    {
        return WidgetInstance;
    }

    /// @see WidgetWindowController.as::get rootWindow
    public IWindow? RootWindow
    {
        get => numChildren > 0 ? GetChildAt(0) : null;
        set
        {
            if (numChildren > 0)
            {
                RemoveChildAt(0);
            }

            if (value == null)
            {
                return;
            }

            AddChild(value);

            if (!value.tags.Contains(TAG_EXCLUDE))
            {
                value.tags.Add(TAG_EXCLUDE);
            }
        }
    }

    /// @see IWidgetWindow — explicit interface
    IWindow? IWidgetWindow.RootWindow()
    {
        return RootWindow;
    }

    /// @see IWidgetWindow — explicit interface
    void IWidgetWindow.RootWindow(object? value)
    {
        RootWindow = value as IWindow;
    }
}
