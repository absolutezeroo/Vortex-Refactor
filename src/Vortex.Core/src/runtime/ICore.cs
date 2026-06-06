// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as

using System.Xml.Linq;

using Vortex.Core.Runtime.Events;
using Vortex.Core.Utils;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as
public interface ICore : IContext, ICoreConfiguration
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::initialize
    void Initialize();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::purge
    void Purge();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::hibernate
    void Hibernate(int param1, int param2 = 1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::resume
    void Resume();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::get fileProxy
    IFileProxy? fileProxy { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::writeDictionaryToProxy
    bool WriteDictionaryToProxy(string param1, Dictionary<string, object?> param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::readDictionaryFromProxy
    Dictionary<string, object?>? ReadDictionaryFromProxy(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::writeXMLToProxy
    bool WriteXmlToProxy(string param1, XElement param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::readXMLFromProxy
    XElement? ReadXmlFromProxy(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::readStringFromProxy
    string? ReadStringFromProxy(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::writeStringToProxy
    bool WriteStringToProxy(string param1, string param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::readConfigDocument
    void ReadConfigDocument(XElement param1, EventDispatcherWrapper? param2 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::getNumberOfFilesPending
    uint GetNumberOfFilesPending();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::getNumberOfFilesLoaded
    uint GetNumberOfFilesLoaded();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::setProfilerMode
    void SetProfilerMode(bool param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::get arguments
    Dictionary<string, object?> arguments { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ICore.as::clearArguments
    void ClearArguments();
}
