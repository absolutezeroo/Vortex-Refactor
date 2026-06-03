// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as

using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Window.Events;

namespace Vortex.Core.Window;

/// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as
public interface ICoreWindowManager
{
    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::create
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

    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::destroy
    void Destroy(IWindow param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::buildFromXML
    IWindow? BuildFromXml(XElement param1, uint param2 = 1, Dictionary<string, object?>? param3 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::windowToXMLString
    string WindowToXmlString(IWindow param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::findWindowByName
    IWindow? FindWindowByName(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::findWindowByTag
    IWindow? FindWindowByTag(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/window/ICoreWindowManager.as::groupWindowsWithTag
    uint GroupWindowsWithTag(string param1, IList<IWindow> param2, int param3 = 0);
}
