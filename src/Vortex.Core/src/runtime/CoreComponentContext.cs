// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Assets;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Utils;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as
public sealed class CoreComponentContext : ComponentContext, ICore
{
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::NUM_UPDATE_RECEIVER_LEVELS
    private const uint NUM_UPDATE_RECEIVER_LEVELS = 3;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CONFIG_XML_NODE_ASSET_LIBRARIES
    private const string CONFIG_XML_NODE_ASSET_LIBRARIES = "asset-libraries";

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CONFIG_XML_NODE_ASSET_LIBRARY
    private const string CONFIG_XML_NODE_ASSET_LIBRARY = "library";

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CONFIG_XML_NODE_SERVICE_LIBRARIES
    private const string CONFIG_XML_NODE_SERVICE_LIBRARIES = "service-libraries";

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CONFIG_XML_NODE_SERVICE_LIBRARY
    private const string CONFIG_XML_NODE_SERVICE_LIBRARY = "library";

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CONFIG_XML_NODE_COMPONENT_LIBRARIES
    private const string CONFIG_XML_NODE_COMPONENT_LIBRARIES = "component-libraries";

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CONFIG_XML_NODE_COMPONENT_LIBRARY
    private const string CONFIG_XML_NODE_COMPONENT_LIBRARY = "library";

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::var_1203
    private static IClass67? var_1203;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::hasLockedComponents
    private static readonly FieldInfo? AttachedComponentsField =
        typeof(ComponentContext).GetField("_attachedComponents", BindingFlags.NonPublic | BindingFlags.Instance);
    private readonly Node? _displayObjectContainer;
    private readonly uint[] var_127;
    private readonly ICoreErrorReporter var_507;
    private readonly uint var_626;

    private readonly List<List<IUpdateReceiver>> var_68;
    private int _hibernationLevel = -1;
    private uint _hibernationUpdateFrequency;
    private ulong _lastUpdateTimeMs;

    private EventDispatcherWrapper? _loadingEventDelegate;
    private LibraryLoaderQueue? _libraryLoaderQueue;
    private uint _numberOfFilesInConfig;
    private uint _numberOfFilesPending;
    private bool _rebootOnNextFrame;
    private SceneTree? _sceneTree;
    private Action<uint, uint> var_37;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::CoreComponentContext
    public CoreComponentContext(Node? param1, ICoreErrorReporter param2, uint param3, Dictionary<string, object?>? param4)
        : base(null, COMPONENT_FLAG_CONTEXT, new AssetLibraryCollection("_core_assets"))
    {
        arguments = param4 != null ? CloneArguments(param4) : new Dictionary<string, object?>(StringComparer.Ordinal);
        var_626 = param3;
        var_154 = (param3 & CoreEnvironment.CORE_SETUP_DEBUG) == CoreEnvironment.CORE_SETUP_DEBUG;
        var_68 = [];
        var_127 = new uint[NUM_UPDATE_RECEIVER_LEVELS];
        _displayObjectContainer = param1;
        var_507 = param2;
        var_37 = SimpleFrameUpdateHandler;

        for (int i = 0;
             i < NUM_UPDATE_RECEIVER_LEVELS;
             i++)
        {
            var_68.Add([]);
            var_127[i] = 0;
        }

        _lastUpdateTimeMs = Time.GetTicksMsec();
        AttachComponent(this, [new IIDCore()]);

        if (_displayObjectContainer != null)
        {
            _sceneTree = _displayObjectContainer.GetTree();
            if (_sceneTree != null)
            {
                _sceneTree.ProcessFrame += OnProcessFrame;
            }
        }

        switch (param3 & CoreEnvironment.CORE_SETUP_FRAME_UPDATE_MASK)
        {
            case CoreEnvironment.CORE_SETUP_FRAME_UPDATE_SIMPLE:
                Debug("Core; using simple frame update handler");
                var_37 = SimpleFrameUpdateHandler;
                break;
            case CoreEnvironment.CORE_SETUP_FRAME_UPDATE_COMPLEX:
                Debug("Core; using complex frame update handler");
                var_37 = ComplexFrameUpdateHandler;
                break;
            case CoreEnvironment.CORE_SETUP_FRAME_UPDATE_PROFILER:
                Debug("Core; using profiler frame update handler");
                var_37 = ProfilerFrameUpdateHandler;
                // TODO(window-port): Port Profiler + ProfilerViewer and attach IIDProfiler.
                break;
            case CoreEnvironment.CORE_SETUP_FRAME_UPDATE_EXPERIMENT:
                Debug("Core; using experimental frame update handler");
                var_37 = ExperimentalFrameUpdateHandler;
                break;
            default:
                Debug("Core; using debug frame update handler");
                var_37 = DebugFrameUpdateHandler;
                break;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::hibernating
    private bool hibernating => _hibernationLevel > -1;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::maxPriority
    private uint maxPriority => hibernating ? (uint)Math.Max(0, _hibernationLevel + 1) : NUM_UPDATE_RECEIVER_LEVELS;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::set errorLogger
    public ICoreErrorLogger? errorLogger
    {
        set => var_507.errorLogger = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::set fileProxy
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::get fileProxy
    public IClass67? fileProxy
    {
        get => var_1203;
        set => var_1203 = value;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::getNumberOfFilesPending
    public uint GetNumberOfFilesPending()
    {
        return _numberOfFilesPending;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::getNumberOfFilesLoaded
    public uint GetNumberOfFilesLoaded()
    {
        return _numberOfFilesInConfig >= _numberOfFilesPending ? _numberOfFilesInConfig - _numberOfFilesPending : 0;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::get arguments
    public Dictionary<string, object?> arguments { get; private set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::clearArguments
    public void ClearArguments()
    {
        arguments = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::initialize
    public void Initialize()
    {
        if (HasLockedComponents())
        {
            events.AddEventListener(COMPONENT_EVENT_UNLOCKED, UnlockEventHandler);
        }
        else
        {
            DoInitialize();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        Debug("Disposing core");
        PurgeTrigger.Stop();

        // @see CoreComponentContext.as — dispose LibraryLoaderQueue
        if (_libraryLoaderQueue != null)
        {
            _libraryLoaderQueue.Dispose();
            _libraryLoaderQueue = null;
        }

        foreach (List<IUpdateReceiver> receivers in var_68)
        {
            receivers.Clear();
        }

        if (_sceneTree != null)
        {
            _sceneTree.ProcessFrame -= OnProcessFrame;
            _sceneTree = null;
        }

        _loadingEventDelegate = null;

        base.Dispose();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::error
    void IContext.Error(string param1, bool param2, int param3, Exception? param4)
    {
        Error(param1, param2, param3, param4);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::readConfigDocument
    public void ReadConfigDocument(XElement param1, EventDispatcherWrapper? param2 = null)
    {
        Debug("Parsing config document");

        _loadingEventDelegate = param2;
        _numberOfFilesInConfig = 0;
        _numberOfFilesPending = 0;

        LoadLibraries(param1, CONFIG_XML_NODE_ASSET_LIBRARIES, CONFIG_XML_NODE_ASSET_LIBRARY);
        LoadLibraries(param1, CONFIG_XML_NODE_SERVICE_LIBRARIES, CONFIG_XML_NODE_SERVICE_LIBRARY);
        LoadLibraries(param1, CONFIG_XML_NODE_COMPONENT_LIBRARIES, CONFIG_XML_NODE_COMPONENT_LIBRARY);

        UpdateLoadingProcess();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::writeDictionaryToProxy
    public bool WriteDictionaryToProxy(string param1, Dictionary<string, object?> param2)
    {
        return WriteObjectToProxy(param1, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::readDictionaryFromProxy
    public Dictionary<string, object?>? ReadDictionaryFromProxy(string param1)
    {
        object? raw = ReadObjectFromProxy(param1);
        return ToDictionary(raw);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::writeXMLToProxy
    public bool WriteXmlToProxy(string param1, XElement param2)
    {
        return WriteStringToProxy(param1, param2.ToString(SaveOptions.DisableFormatting));
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::readXMLFromProxy
    public XElement? ReadXmlFromProxy(string param1)
    {
        string? xmlText = ReadStringFromProxy(param1);
        if (string.IsNullOrEmpty(xmlText))
        {
            return null;
        }

        try
        {
            return XElement.Parse(xmlText);
        }
        catch (Exception e)
        {
            Logger.Error($"Caught error when reading XML ({param1}) from IFileProxy", e);
            return null;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::readStringFromProxy
    public string? ReadStringFromProxy(string param1)
    {
        try
        {
            if (var_1203 == null)
            {
                return null;
            }

            byte[]? payload = var_1203.ReadCache(param1);
            return payload == null ? null : Encoding.UTF8.GetString(payload);
        }
        catch (Exception e)
        {
            Logger.Error($"Caught error when reading Object ({param1}) from IFileProxy", e);
            return null;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::writeStringToProxy
    public bool WriteStringToProxy(string param1, string param2)
    {
        try
        {
            if (var_1203 == null)
            {
                return false;
            }

            var_1203.WriteCache(param1, Encoding.UTF8.GetBytes(param2));

            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Caught error when writing String ({param1}) to IFileProxy", e);
            return false;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::registerUpdateReceiver
    void IContext.RegisterUpdateReceiver(IUpdateReceiver param1, uint param2)
    {
        RegisterUpdateReceiver(param1, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::removeUpdateReceiver
    void IContext.RemoveUpdateReceiver(IUpdateReceiver param1)
    {
        RemoveUpdateReceiver(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::hibernate
    public void Hibernate(int param1, int param2 = 1)
    {
        if (hibernating)
        {
            return;
        }

        PurgeTrigger.Stop();

        _hibernationLevel = param1;
        _hibernationUpdateFrequency = (uint)(1000 / Math.Max(1, param2));
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::resume
    public void Resume()
    {
        if (!hibernating)
        {
            return;
        }

        PurgeTrigger.Start();

        _hibernationLevel = -1;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::setProfilerMode
    public void SetProfilerMode(bool param1)
    {
        if (param1)
        {
            var_37 = ProfilerFrameUpdateHandler;
            // TODO(window-port): Port Profiler instance + IIDProfiler registration.
        }
        else
        {
            var_37 = (var_626 & CoreEnvironment.CORE_SETUP_FRAME_UPDATE_MASK) switch
            {
                CoreEnvironment.CORE_SETUP_FRAME_UPDATE_SIMPLE => SimpleFrameUpdateHandler,
                CoreEnvironment.CORE_SETUP_FRAME_UPDATE_COMPLEX => ComplexFrameUpdateHandler,
                CoreEnvironment.CORE_SETUP_FRAME_UPDATE_EXPERIMENT => ExperimentalFrameUpdateHandler,
                _ => DebugFrameUpdateHandler,
            };
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::reboot
    public void Reboot()
    {
        _rebootOnNextFrame = true;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::writeObjectToProxy
    private static bool WriteObjectToProxy(string param1, object param2)
    {
        try
        {
            if (var_1203 == null)
            {
                return false;
            }

            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(param2);
            var_1203.WriteCache(param1, payload);
            return true;
        }
        catch (Exception e)
        {
            Logger.Error($"Caught error when writing Object ({param1}) to IFileProxy", e);
            return false;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::readObjectFromProxy
    private static object? ReadObjectFromProxy(string param1)
    {
        try
        {
            if (var_1203 == null)
            {
                return null;
            }

            byte[]? payload = var_1203.ReadCache(param1);
            if (payload == null || payload.Length == 0)
            {
                return null;
            }

            string json = Encoding.UTF8.GetString(payload);
            return JsonSerializer.Deserialize<object?>(json);
        }
        catch (Exception e)
        {
            Logger.Error($"Caught error when reading Object ({param1}) from IFileProxy", e);
            return null;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::unlockEventHandler
    private void UnlockEventHandler(object? param1)
    {
        _ = param1;

        if (HasLockedComponents())
        {
            return;
        }

        events.RemoveEventListener(COMPONENT_EVENT_UNLOCKED, UnlockEventHandler);

        DoInitialize();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::doInitialize
    private void DoInitialize()
    {
        events.DispatchEvent(COMPONENT_EVENT_RUNNING);
        PurgeTrigger.Start();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::hasLockedComponents
    public bool HasLockedComponents()
    {
        return AttachedComponentsField?.GetValue(this) is List<Component> attachedComponents &&
               attachedComponents.Any(component => component.locked);

    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::error
    public new void Error(string param1, bool param2, int param3 = -1, Exception? param4 = null)
    {
        base.Error(param1, param2, param3, param4);

        var_507.LogError(param1, param2, param3, param4);

        if (param2 && param3 != 2015)
        {
            Dispose();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::loadLibraries
    private void LoadLibraries(XElement configXML, string libraryNodeName, string itemNodeName)
    {
        XElement? section = configXML.Element(libraryNodeName);
        if (section == null)
        {
            return;
        }

        _libraryLoaderQueue ??= new LibraryLoaderQueue();

        foreach (XElement libraryNode in section.Elements(itemNodeName))
        {
            string url = libraryNode.Attribute("url")?.Value ?? string.Empty;
            _numberOfFilesInConfig++;
            _numberOfFilesPending++;

            /// @see CoreComponentContext.as::loadLibraries — AS3 creates a LibraryLoader,
            /// calls assets.loadFromFile(loader, true), then loads from URLRequest.
            /// Godot adaptation: register the library URL with the asset collection.
            if (libraryNodeName == CONFIG_XML_NODE_ASSET_LIBRARIES && assets is IAssetLibrary assetLibrary)
            {
                assetLibrary.LoadFromFile(url, true);
            }

            // @see CoreComponentContext.as — enqueue into LibraryLoaderQueue with completion callback
            _libraryLoaderQueue.Push(
                url, (loadedUrl, _) =>
                {
                    _numberOfFilesPending = _numberOfFilesPending > 0 ? _numberOfFilesPending - 1 : 0;
                    UpdateLoadingProgress(loadedUrl);
                    UpdateLoadingProcess();
                }
            );
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::updateLoadingProgress
    private void UpdateLoadingProgress(string url)
    {
        if (_loadingEventDelegate == null)
        {
            return;
        }

        _loadingEventDelegate.DispatchEvent(
            "progress", new
            {
                library = url,
                loaded = GetNumberOfFilesLoaded(),
                total = _numberOfFilesInConfig,
            }
        );
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::finalizeLoadingEventDelegate
    private void FinalizeLoadingEventDelegate()
    {
        if (_loadingEventDelegate == null)
        {
            return;
        }

        _loadingEventDelegate.DispatchEvent("complete");
        _loadingEventDelegate = null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::updateLoadingProcess
    private void UpdateLoadingProcess()
    {
        if (disposed)
        {
            return;
        }

        if (_numberOfFilesPending != 0)
        {
            return;
        }

        FinalizeLoadingEventDelegate();

        Debug("All libraries loaded, Core is now running");
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::registerUpdateReceiver
    public new void RegisterUpdateReceiver(IUpdateReceiver param1, uint param2)
    {
        RemoveUpdateReceiver(param1);

        uint cappedPriority = param2 >= NUM_UPDATE_RECEIVER_LEVELS ? NUM_UPDATE_RECEIVER_LEVELS - 1 : param2;

        var_68[(int)cappedPriority].Add(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::removeUpdateReceiver
    public new void RemoveUpdateReceiver(IUpdateReceiver param1)
    {
        if (disposed)
        {
            return;
        }

        foreach (List<IUpdateReceiver> receivers in var_68)
        {
            receivers.RemoveAll(receiver => ReferenceEquals(receiver, param1));
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::onEnterFrame
    private void OnProcessFrame()
    {
        if (disposed)
        {
            return;
        }

        if (_rebootOnNextFrame)
        {
            _rebootOnNextFrame = false;
            events.DispatchEvent(COMPONENT_EVENT_REBOOT);
            return;
        }

        uint nowMs = (uint)Time.GetTicksMsec();
        uint deltaMs = (uint)(nowMs - _lastUpdateTimeMs);

        if (hibernating && deltaMs <= _hibernationUpdateFrequency)
        {
            return;
        }

        var_37(nowMs, deltaMs);

        _lastUpdateTimeMs = nowMs;

        // @see class_1595.as::onInterval — AS3 uses setTimeout self-recursion;
        // Godot adaptation: poll once per frame (lightweight timestamp check).
        PurgeTrigger.CheckAndTrigger();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::simpleFrameUpdateHandler
    private void SimpleFrameUpdateHandler(uint param1, uint param2)
    {
        _ = param1;
        for (int priority = 0;
             priority < maxPriority;
             priority++)
        {
            var_127[priority] = 0;
            List<IUpdateReceiver> receivers = var_68[priority];
            int index = 0;

            while (index < receivers.Count)
            {
                IUpdateReceiver? receiver = receivers[index];

                if (receiver == null || receiver.disposed)
                {
                    receivers.RemoveAt(index);
                    continue;
                }

                try
                {
                    receiver.Update(param2);
                }
                catch (Exception e)
                {
                    Error($"Error in update receiver \"{receiver.GetType().FullName}\": {e.Message}", true, e.HResult, e);
                    return;
                }

                index++;
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::complexFrameUpdateHandler
    private void ComplexFrameUpdateHandler(uint param1, uint param2)
    {
        uint frameBudgetMs = (uint)(1000 / 60);
        bool keepRunning = true;

        for (int priority = 0;
             priority < maxPriority;
             priority++)
        {
            uint loopTimer = (uint)Time.GetTicksMsec();
            bool skipLevel = false;

            if (loopTimer - param1 > frameBudgetMs && var_127[priority] < priority)
            {
                var_127[priority]++;
                skipLevel = true;
            }

            if (skipLevel)
            {
                continue;
            }

            var_127[priority] = 0;
            List<IUpdateReceiver> receivers = var_68[priority];
            int index = 0;

            while (index < receivers.Count && keepRunning)
            {
                IUpdateReceiver? receiver = receivers[index];

                if (receiver == null || receiver.disposed)
                {
                    receivers.RemoveAt(index);
                    continue;
                }

                try
                {
                    receiver.Update(param2);
                }
                catch (Exception e)
                {
                    Error($"Error in update receiver \"{receiver.GetType().FullName}\": {e}", true, e.HResult, e);
                    keepRunning = false;
                }

                index++;
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::profilerFrameUpdateHandler
    private void ProfilerFrameUpdateHandler(uint param1, uint param2)
    {
        _ = param1;
        // TODO(window-port): Port Profiler integration and per-receiver timing.
        SimpleFrameUpdateHandler(param1, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::experimentalFrameUpdateHandler
    private void ExperimentalFrameUpdateHandler(uint param1, uint param2)
    {
        _ = param1;
        _ = param2;

        foreach (List<IUpdateReceiver> receivers in var_68)
        {
            receivers.RemoveAll(receiver => receiver == null || receiver.disposed);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::debugFrameUpdateHandler
    private void DebugFrameUpdateHandler(uint param1, uint param2)
    {
        _ = param1;
        for (int priority = 0;
             priority < maxPriority;
             priority++)
        {
            var_127[priority] = 0;
            List<IUpdateReceiver> receivers = var_68[priority];
            int index = 0;

            while (index < receivers.Count)
            {
                IUpdateReceiver? receiver = receivers[index];

                if (receiver == null || receiver.disposed)
                {
                    receivers.RemoveAt(index);
                    continue;
                }

                receiver.Update(param2);
                index++;
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::clone
    private static Dictionary<string, object?> CloneArguments(Dictionary<string, object?> param1)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, object?> pair in param1)
        {
            if (pair.Value is Dictionary<string, object?> nested)
            {
                result[pair.Key] = CloneArguments(nested);
            }
            else
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/CoreComponentContext.as::readDictionaryFromProxy
    private static Dictionary<string, object?>? ToDictionary(object? value)
    {
        return value switch
        {
            null => null,
            Dictionary<string, object?> typedDictionary => typedDictionary,
            JsonElement { ValueKind: JsonValueKind.Object } jsonElement => JsonSerializer.Deserialize<Dictionary<string, object?>>(
                jsonElement.GetRawText()),
            JsonElement => null,
            string jsonString => JsonSerializer.Deserialize<Dictionary<string, object?>>(jsonString),
            _ => null,
        };

    }
}
