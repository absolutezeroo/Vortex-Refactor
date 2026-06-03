// @see core/window/components/IconController.as

using System;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Graphics.Renderer;

namespace Vortex.Core.Window.Components;

/// @see core/window/components/IconController.as
public class IconController : WindowController, IIconWindow
{
    /// @see IconController.as::IconController (default)
    public IconController() : base() { }

    /// @see IconController.as::IconController (name + rect)
    public IconController(string param1, Rect2 param2) : base(param1, param2) { }

    /// @see IconController.as::IconController (full AS3 11-param signature)
    public IconController
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

    /// @see IconController.as::set style
    public new uint style
    {
        get => base.style;
        set => base.style = value;
    }

    /// @see IconController.as::fitToSize
    public void FitToSize()
    {
        if (_context is not WindowContext)
        {
            return;
        }

        ISkinRenderer? skinRenderer = WindowContext.GetRenderer()?.SkinContainer.GetSkinRendererByTypeAndStyle(1, style);

        if (skinRenderer is not BitmapSkinRenderer bitmapRenderer)
        {
            return;
        }

        ISkinLayout? layout = bitmapRenderer.GetLayoutByState(state);

        if (layout == null)
        {
            return;
        }

        int layoutWidth = (int)layout.Width;
        int layoutHeight = (int)layout.Height;

        if (layoutWidth == (int)width && layoutHeight == (int)height)
        {
            return;
        }

        width = layoutWidth;
        height = layoutHeight;
    }

    /// @see IIconWindow — explicit interface implementation
    void IIconWindow.FitToSize()
    {
        FitToSize();
    }
}
