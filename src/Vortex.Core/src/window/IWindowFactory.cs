// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window.Events;
using Vortex.Core.Window.Theme;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as
public interface IWindowFactory
{
    /// @see class_1799.as::getThemeManager
    IThemeManager? GetThemeManager();

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::create
    IWindow? Create
    (
        string param1,
        uint param2,
        uint param3,
        uint param4,
        Rect2 param5,
        Action<WindowEvent, IWindow>? param6 = null,
        string param7 = "",
        uint param8 = 0,
        IList<string>? param9 = null,
        IWindow? param10 = null,
        IList<object>? param11 = null,
        string param12 = ""
    );

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::destroy
    void Destroy(IWindow param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::buildFromXML
    IWindow? BuildFromXml(XElement param1, uint param2 = 1, Dictionary<string, object?>? param3 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::windowToXMLString
    string WindowToXmlString(IWindow param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::findWindowByName
    IWindow? FindWindowByName(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::findWindowByTag
    IWindow? FindWindowByTag(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/class_1799.as::groupWindowsWithTag
    uint GroupWindowsWithTag(string param1, IList<IWindow> param2, int param3 = 0);
}
