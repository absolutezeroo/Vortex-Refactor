// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as

using System;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Window.Components;
using Vortex.Core.Window.Events;
using Vortex.Core.Window.Utils;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as
public interface IWindowContext
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::create
    IWindow? Create
    (
        string param1,
        string param2,
        uint param3,
        uint param4,
        uint param5,
        Rect2 param6,
        Action<WindowEvent, IWindow>? param7,
        IWindow? param8,
        uint param9,
        IList<object>? param10 = null,
        string param11 = "",
        IList<string>? param12 = null
    );

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::destroy
    bool Destroy(IWindow param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::findWindowByName
    IWindow? FindWindowByName(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::findWindowByTag
    IWindow? FindWindowByTag(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::groupChildrenWithTag
    uint GroupChildrenWithTag(string param1, IList<IWindow> param2, int param3 = 0);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::getDesktopWindow
    IDesktopWindow GetDesktopWindow();

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::getWindowParser
    WindowParser GetWindowParser();

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::getWindowFactory
    IWindowFactory GetWindowFactory();

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::getWidgetFactory
    IWidgetFactory GetWidgetFactory();

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::addMouseEventTracker
    void AddMouseEventTracker(IInputEventTracker param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::removeMouseEventTracker
    void RemoveMouseEventTracker(IInputEventTracker param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::getResourceManager
    IResourceManager? GetResourceManager();

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::registerLocalizationListener
    void RegisterLocalizationListener(string param1, IWindow param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::removeLocalizationListener
    void RemoveLocalizationListener(string param1, IWindow param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_3400.as::invalidate
    void Invalidate(IWindow param1, Rect2? param2, uint param3);
}
