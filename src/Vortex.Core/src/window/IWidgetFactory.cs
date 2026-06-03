// @see WIN63-202407091256-704579380-Source-main/core/window/class_1798.as

using Vortex.Core.Window.Components;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/class_1798.as
public interface IWidgetFactory
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1798.as::createWidget
    IWidget? CreateWidget(string param1, IDesktopWindow param2);
}
