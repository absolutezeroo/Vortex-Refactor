// @see WIN63-202111081545-75921380-Source-main/src/FakeContext.as

using System;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;

namespace Vortex;

/// @see WIN63-202111081545-75921380-Source-main/src/FakeContext.as
/// Lightweight IContext stub used by HabboLoadingScreen to bootstrap configuration/localization
/// before the real CoreComponentContext is created.
public class FakeContext : IContext
{
    private EventDispatcherWrapper? _events;
    private Dictionary<string, object?>? _arguments;
    private string _lastError = string.Empty;
    private string _lastDebug = string.Empty;
    private string _lastWarning = string.Empty;

    /// @see FakeContext.as::FakeContext
    public FakeContext(Dictionary<string, object?>? properties)
    {
        _events = new EventDispatcherWrapper();
        AssetLibraryCollection = new AssetLibraryCollection("fakeAssetCollection");
        _arguments = properties != null
            ? new Dictionary<string, object?>(properties, StringComparer.Ordinal)
            : new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    /// @see FakeContext.as::get assets
    public object? assets => AssetLibraryCollection;

    /// @see FakeContext.as::get events
    public EventDispatcherWrapper events => _events!;

    /// @see FakeContext.as::get root
    public IContext root => this;

    /// @see FakeContext.as::get configuration
    public ICoreConfiguration? configuration { get; set; }

    /// @see FakeContext.as::get displayObjectContainer
    public object? displayObjectContainer => null;

    public bool disposed { get; private set; }

    /// @see FakeContext.as::get linkEventTrackers
    public IList<ILinkEventTracker> linkEventTrackers { get; } = [];

    /// Provides access to the arguments dictionary for configuration parsing.
    public Dictionary<string, object?> arguments => _arguments ?? [];

    /// Provides typed access to the asset library collection.
    public AssetLibraryCollection? AssetLibraryCollection { get; private set; }

    /// @see FakeContext.as::error
    public void Error(string param1, bool param2, int param3 = -1, Exception? param4 = null)
    {
        _lastError = param1;
    }

    /// @see FakeContext.as::getLastErrorMessage
    public string GetLastErrorMessage()
    {
        return _lastError;
    }

    /// @see FakeContext.as::debug
    public void Debug(string param1)
    {
        _lastDebug = param1;
    }

    /// @see FakeContext.as::getLastDebugMessage
    public string GetLastDebugMessage()
    {
        return _lastDebug;
    }

    /// @see FakeContext.as::warning
    public void Warning(string param1)
    {
        _lastWarning = param1;
    }

    /// @see FakeContext.as::getLastWarningMessage
    public string GetLastWarningMessage()
    {
        return _lastWarning;
    }

    /// @see FakeContext.as::loadFromFile
    public object? LoadFromFile(object param1, object? param2)
    {
        return null;
    }

    /// @see FakeContext.as::attachComponent
    public void AttachComponent(Component param1, IList<Core.Runtime.IID> param2) { }

    /// @see FakeContext.as::detachComponent
    public void DetachComponent(Component param1) { }

    /// @see FakeContext.as::prepareComponent
    public IUnknown? PrepareComponent(Type param1, uint param2 = 0, object? param3 = null)
    {
        return null;
    }

    /// @see FakeContext.as::prepareAssetLibrary
    public bool PrepareAssetLibrary(XElement param1, Type param2)
    {
        return false;
    }

    /// @see FakeContext.as::registerUpdateReceiver
    public void RegisterUpdateReceiver(IUpdateReceiver param1, uint param2) { }

    /// @see FakeContext.as::removeUpdateReceiver
    public void RemoveUpdateReceiver(IUpdateReceiver param1) { }

    /// @see FakeContext.as::toXMLString
    public string ToXmlString(uint param1 = 0)
    {
        return "<FakeContext/>";
    }

    /// @see FakeContext.as::addLinkEventTracker
    public void AddLinkEventTracker(ILinkEventTracker param1) { }

    /// @see FakeContext.as::removeLinkEventTracker
    public void RemoveLinkEventTracker(ILinkEventTracker param1) { }

    /// @see FakeContext.as::createLinkEvent
    public void CreateLinkEvent(string param1) { }

    /// @see FakeContext.as::queueInterface
    public IUnknown? QueueInterface(Core.Runtime.IID param1, Action<Core.Runtime.IID, IUnknown?>? param2 = null)
    {
        return null;
    }

    /// @see FakeContext.as::release
    public uint Release(Core.Runtime.IID param1)
    {
        return 0;
    }

    /// @see FakeContext.as::dispose
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        _events?.Dispose();
        _events = null;

        AssetLibraryCollection?.Dispose();
        AssetLibraryCollection = null;

        _arguments = null;
        configuration = null;
    }
}
