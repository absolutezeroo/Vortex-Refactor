// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as

using System.Xml.Linq;

using Vortex.Core.Runtime.Events;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as
public interface IContext : IUnknown
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get assets
    object? assets { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get events
    EventDispatcherWrapper events { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get root
    IContext root { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::error
    void Error(string param1, bool param2, int param3 = -1, System.Exception? param4 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::getLastErrorMessage
    string GetLastErrorMessage();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::debug
    void Debug(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::getLastDebugMessage
    string GetLastDebugMessage();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::warning
    void Warning(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::getLastWarningMessage
    string GetLastWarningMessage();

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get displayObjectContainer
    object? displayObjectContainer { get; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::loadFromFile
    object? LoadFromFile(object param1, object? param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::attachComponent
    void AttachComponent(Component param1, IList<IID> param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::detachComponent
    void DetachComponent(Component param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::prepareComponent
    IUnknown? PrepareComponent(System.Type param1, uint param2 = 0, object? param3 = null);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::prepareAssetLibrary
    bool PrepareAssetLibrary(XElement param1, System.Type param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::registerUpdateReceiver
    void RegisterUpdateReceiver(IUpdateReceiver param1, uint param2);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::removeUpdateReceiver
    void RemoveUpdateReceiver(IUpdateReceiver param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::toXMLString
    string ToXmlString(uint param1 = 0);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get configuration
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::set configuration
    ICoreConfiguration? configuration { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::addLinkEventTracker
    void AddLinkEventTracker(ILinkEventTracker param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::removeLinkEventTracker
    void RemoveLinkEventTracker(ILinkEventTracker param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::createLinkEvent
    void CreateLinkEvent(string param1);

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get linkEventTrackers
    IList<ILinkEventTracker> linkEventTrackers { get; }
}
