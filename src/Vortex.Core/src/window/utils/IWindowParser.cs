// @see core/window/utils/class_3577.as

using System.Xml.Linq;

namespace Vortex.Core.Window.Utils;

/// @see core/window/utils/class_3577.as
public interface IWindowParser
{
    /// @see core/window/utils/class_3577.as::get disposed
    bool disposed { get; }

    /// @see core/window/utils/class_3577.as::parseAndConstruct
    IWindow? ParseAndConstruct(XElement param1, IWindow? param2 = null, Dictionary<string, object?>? param3 = null);

    /// @see core/window/utils/class_3577.as::windowToXMLString
    string WindowToXmlString(IWindow param1);

    /// @see core/window/utils/class_3577.as::dispose
    void Dispose();
}
