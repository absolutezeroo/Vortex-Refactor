// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

using Vortex.Core.Assets;
using Vortex.Core.Runtime.Events;
using Vortex.Core.Runtime.Exceptions;

using Exception = System.Exception;

namespace Vortex.Core.Runtime;

/// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as
public class ComponentContext : Component, IContext
{
    private readonly List<Component> _attachedComponents;
    private readonly List<ComponentInterfaceQueue> _interfacesQueued;
    private readonly List<ILinkEventTracker> _linkEventTrackers;
    private readonly SortedDictionary<uint, List<IUpdateReceiver>> _updateReceivers;

    private static readonly Dictionary<string, Type> _manifestTypeLookup = new(StringComparer.Ordinal);
    private static readonly HashSet<string> _scannedAssemblies = new(StringComparer.Ordinal);

    protected bool var_154 = true;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::ComponentContext
    public ComponentContext(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2 | COMPONENT_FLAG_CONTEXT, param3)
    {
        displayObjectContainer = new object();
        _attachedComponents = [];
        _interfacesQueued = [];
        _linkEventTrackers = [];
        _updateReceivers = new SortedDictionary<uint, List<IUpdateReceiver>>();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get root
    public IContext root
    {
        get
        {
            if (context == null || ReferenceEquals(context, this))
            {
                return this;
            }

            return context.root;
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get displayObjectContainer
    public object? displayObjectContainer { get; protected set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::purge
    public override void Purge()
    {
        base.Purge();

        foreach (Component component in _attachedComponents.Where(component => !ReferenceEquals(component, this)))
        {
            component.Purge();
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::debug
    public void Debug(string param1)
    {
        _lastDebugMessage = param1;

        if (var_154)
        {
            events.DispatchEvent(COMPONENT_EVENT_DEBUG, param1);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::getLastDebugMessage
    public string GetLastDebugMessage()
    {
        return _lastDebugMessage;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::warning
    public void Warning(string param1)
    {
        _lastWarningMessage = param1;
        events.DispatchEvent(COMPONENT_EVENT_WARNING, param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::getLastWarningMessage
    public string GetLastWarningMessage()
    {
        return _lastWarningMessage;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::error
    public void Error(string param1, bool param2, int param3 = -1, Exception? param4 = null)
    {
        _lastError = param1;
        Logger.Error($"[ComponentContext] Error: {param1} (critical={param2}, cat={param3})", param4);
        events.DispatchEvent(
            COMPONENT_EVENT_ERROR, new
            {
                message = param1,
                critical = param2,
                category = param3,
                error = param4,
            }
        );
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::getLastErrorMessage
    public string GetLastErrorMessage()
    {
        return _lastError;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::loadFromFile
    /// @see ComponentContext.as::loadFromFile
    /// Godot adaptation: LibraryLoader not ported. Returns null — file-based loading
    /// handled via LoadFromResource and direct asset registration.
    public object? LoadFromFile(object param1, object? param2)
    {
        // AS3 creates a LibraryLoader, registers event handlers, and tracks in _loaders vector.
        // LibraryLoader is a Flash SWF loader — no direct Godot equivalent.
        // Consumers should use PrepareAssetLibrary(manifest, resourceClass) instead.
        return null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::prepareAssetLibrary
    /// @see ComponentContext.as::prepareAssetLibrary — `return assets.loadFromResource(param1, param2);`
    public bool PrepareAssetLibrary(XElement param1, Type param2)
    {
        if (assets is IAssetLibrary assetLibrary)
        {
            return assetLibrary.LoadFromResource(param1, param2);
        }

        // If no asset library is set, create one and store it.
        AssetLibrary library = new(GetType().Name + "_assets");
        bool result = library.LoadFromResource(param1, param2);
        assets = library;

        return result;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::prepareComponent
    public IUnknown? PrepareComponent(Type param1, uint param2 = 0, object? param3 = null)
    {
        if (TryGetManifestXml(param1, out XElement manifestXml))
        {
            return PrepareManifestComponents(param1, manifestXml, param2, param3);
        }

        return typeof(Component).IsAssignableFrom(param1) ? PrepareRuntimeComponent(param1, param2, param3) : null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::prepareComponent
    private IUnknown? PrepareRuntimeComponent(Type param1, uint param2, object? param3)
    {
        Component? component = CreateComponentInstance(param1, param2, param3);
        if (component == null)
        {
            Error("Invalid component class " + param1.FullName + "!", true, 4);
            return null;
        }

        List<IID> interfaces = new();
        InterfaceStructList? interfaceList = GetInterfaceStructList(component);

        if (interfaceList != null)
        {
            for (uint i = 0;
                 i < interfaceList.length;
                 i++)
            {
                InterfaceStruct? @struct = interfaceList.GetStructByIndex(i);

                if (@struct?.iid != null)
                {
                    interfaces.Add(@struct.iid);
                }
            }
        }

        AttachComponent(component, interfaces);
        return component;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/runtime/ComponentContext.as::prepareComponent
    private IUnknown? PrepareManifestComponents(Type param1, XElement param2, uint param3, object? param4)
    {
        Component? lastPreparedComponent = null;

        // Ensure the caller's assembly is scanned (in case RegisterManifestAssembly wasn't called for it)
        RegisterManifestAssembly(param1.Assembly);

        foreach (XElement componentElement in param2.Elements("component"))
        {
            string componentClassName = componentElement.Attribute("class")?.Value ?? string.Empty;
            Type? componentType = ResolveManifestType(componentClassName);

            if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            {
                Error("Invalid component class " + componentClassName + "!", true, 4);

                return null;
            }

            // @see ComponentContext.as lines 228-238 — extract <assets> and <aliases> from manifest,
            // create a per-component AssetLibrary, and pass it as param3 to the component constructor.
            List<XElement> assetsElements = componentElement.Elements("assets").ToList();
            List<XElement> aliasesElements = componentElement.Elements("aliases").ToList();
            IAssetLibrary? componentAssetLibrary = null;

            if (assetsElements.Count > 0)
            {
                XElement libraryManifest = new(
                    "manifest",
                    new XElement("library")
                );
                XElement libraryElement = libraryManifest.Element("library")!;

                foreach (XElement assetsElement in assetsElements)
                {
                    libraryElement.Add(assetsElement);
                }

                foreach (XElement aliasesElement in aliasesElements)
                {
                    libraryElement.Add(aliasesElement);
                }

                componentAssetLibrary = new AssetLibrary("_assets@" + componentClassName, libraryManifest);
                componentAssetLibrary.LoadFromResource(libraryManifest, param1);
            }

            Component? componentInstance = CreateComponentInstance(componentType, param3, componentAssetLibrary ?? param4);

            if (componentInstance == null)
            {
                Error("Invalid component class " + componentClassName + "!", true, 4);

                return null;
            }

            // @see ComponentContext.as lines 252-258 — validate that the component saved the asset library
            if (componentAssetLibrary != null && componentInstance.assets != componentAssetLibrary)
            {
                componentAssetLibrary.Dispose();
                Error("Component \"" + componentClassName + "\" did not save provided asset library!", true, 4);
            }

            IList<IID> interfaces = CollectComponentInterfaces(componentInstance, componentElement);

            AttachComponent(componentInstance, interfaces);

            lastPreparedComponent = componentInstance;
        }

        return lastPreparedComponent;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/runtime/ComponentContext.as::prepareComponent
    private static IList<IID> CollectComponentInterfaces(Component param1, XElement param2)
    {
        List<IID> interfaces = new();

        foreach (XElement interfaceElement in param2.Elements("interface"))
        {
            string interfaceClassName = interfaceElement.Attribute("iid")?.Value ?? string.Empty;
            Type? interfaceType = ResolveManifestType(interfaceClassName);

            if (interfaceType == null || !typeof(IID).IsAssignableFrom(interfaceType)
                                      || Activator.CreateInstance(interfaceType) is not IID interfaceIdentifier)
            {
                throw new InvalidComponentException("Identifier class defined in manifest not found: " + interfaceClassName);
            }

            GetInterfaceStructList(param1)?.Insert(new InterfaceStruct(interfaceIdentifier, param1));
            interfaces.Add(interfaceIdentifier);
        }

        return interfaces;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/runtime/ComponentContext.as::prepareComponent
    private static bool TryGetManifestXml(Type param1, out XElement param2)
    {
        return TryGetManifestXmlFromStaticProperty(param1, out param2) || TryGetManifestXmlFromSourceBinaryData(param1, out param2);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/runtime/ComponentContext.as::prepareComponent
    private static bool TryGetManifestXmlFromStaticProperty(Type param1, out XElement param2)
    {
        param2 = default!;

        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        PropertyInfo? manifestProperty = param1.GetProperty("manifest", flags);

        if (manifestProperty == null)
        {
            return false;
        }

        try
        {
            object? value = manifestProperty.GetValue(null);

            switch (value)
            {
                case XElement manifestElement:
                    param2 = manifestElement;
                    return true;
                case string manifestXmlText when !string.IsNullOrWhiteSpace(manifestXmlText):
                    param2 = XElement.Parse(manifestXmlText);
                    return true;
                case byte[] { Length: > 0 } manifestBytes:
                    param2 = XElement.Parse(Encoding.UTF8.GetString(manifestBytes));
                    return true;
                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/binaryData/HabboHabboCommunicationCom_Habbomanifest_xml.bin
    private static bool TryGetManifestXmlFromSourceBinaryData(Type param1, out XElement param2)
    {
        param2 = default!;

        string[] manifestDirectories =
        [
            Path.Combine("data", "manifests"),
        ];

        foreach (string manifestDirectory in manifestDirectories)
        {
            if (!Directory.Exists(manifestDirectory))
            {
                continue;
            }

            string canonicalName = "Habbo" + param1.Name + "_manifest_xml";

            string[] candidates = new[]
            {
                Path.Combine(manifestDirectory, canonicalName + ".xml"),
                Path.Combine(manifestDirectory, canonicalName),
            };

            string? manifestPath = candidates.FirstOrDefault(File.Exists)
                                   ?? Directory.EnumerateFiles(manifestDirectory, "*" + param1.Name + "_manifest.xml").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
            {
                continue;
            }

            try
            {
                string manifestXml = File.ReadAllText(manifestPath, Encoding.UTF8);
                param2 = XElement.Parse(manifestXml);
                return true;
            }
            catch
            {
                // Ignore malformed manifest candidate and continue lookup.
            }
        }

        return false;
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/runtime/ComponentContext.as::prepareComponent
    private static Type? ResolveManifestType(string param1)
    {
        if (string.IsNullOrWhiteSpace(param1))
        {
            return null;
        }

        if (_manifestTypeLookup.TryGetValue(param1, out Type? fullNameMatch))
        {
            return fullNameMatch;
        }

        string typeToken = ExtractTypeToken(param1);
        if (_manifestTypeLookup.TryGetValue(typeToken, out Type? tokenMatch))
        {
            return tokenMatch;
        }

        if (!typeToken.EndsWith("Bootstrap", StringComparison.Ordinal))
        {
            return null;
        }

        string fallbackTypeName = typeToken[..^"Bootstrap".Length];

        return _manifestTypeLookup.GetValueOrDefault(fallbackTypeName);
    }

    /// @see WIN63-202111081545-75921380-Source-main/src/com/sulake/core/runtime/ComponentContext.as::prepareComponent
    private static string ExtractTypeToken(string param1)
    {
        int lastDotIndex = param1.LastIndexOf('.');
        return lastDotIndex >= 0 ? param1[(lastDotIndex + 1)..] : param1;
    }

    /// <summary>
    /// Registers an assembly so its types are discoverable by manifest type resolution.
    /// Call this before <see cref="PrepareComponent"/> for each project assembly.
    /// This mirrors AS3's ApplicationDomain where all loaded SWFs are visible to getDefinitionByName.
    /// </summary>
    public static void RegisterManifestAssembly(Assembly assembly)
    {
        string? assemblyName = assembly.GetName().Name;
        if (assemblyName == null || !_scannedAssemblies.Add(assemblyName))
        {
            return;
        }

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            types = e.Types.Where(t => t != null).ToArray()!;
        }

        foreach (Type type in types)
        {
            if (!string.IsNullOrWhiteSpace(type.FullName))
            {
                _manifestTypeLookup.TryAdd(type.FullName, type);
            }

            _manifestTypeLookup.TryAdd(type.Name, type);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::attachComponent
    public void AttachComponent(Component param1, IList<IID> param2)
    {
        if (_attachedComponents.Contains(param1))
        {
            Error("Component " + param1 + " already attached to context!", false);
            return;
        }

        _attachedComponents.Add(param1);

        if (param1.locked)
        {
            param1.events.AddEventListener(INTERNAL_EVENT_UNLOCKED, UnlockEventHandler);
        }

        foreach (IID iid in param2)
        {
            InterfaceStructList? sourceList = GetInterfaceStructList(param1);
            if (sourceList != null && sourceList.Find(iid) == null)
            {
                sourceList.Insert(new InterfaceStruct(iid, param1));
            }

            GetInterfaceStructList(this)?.Insert(new InterfaceStruct(iid, param1));
        }

        if (!param1.locked)
        {
            foreach (IID iid in param2)
            {
                if (HasQueueForInterface(iid))
                {
                    AnnounceInterfaceAvailability(iid, param1);
                }
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::detachComponent
    public void DetachComponent(Component param1)
    {
        InterfaceStructList? ownInterfaceList = GetInterfaceStructList(this);

        if (ownInterfaceList != null)
        {
            int index = ownInterfaceList.GetIndexByImplementor(param1);

            while (index > -1)
            {
                ownInterfaceList.Remove((uint)index);
                index = ownInterfaceList.GetIndexByImplementor(param1);
            }
        }

        param1.events.RemoveEventListener(INTERNAL_EVENT_UNLOCKED, UnlockEventHandler);
        _attachedComponents.Remove(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::queueInterface
    public override IUnknown? QueueInterface(IID param1, Action<IID, IUnknown?>? param2 = null)
    {
        InterfaceStructList? ownInterfaceList = GetInterfaceStructList(this);
        InterfaceStruct? interfaceStruct = ownInterfaceList?.GetStructByInterface(param1);

        if (interfaceStruct != null)
        {
            string iidName = param1.GetType().FullName ?? param1.GetType().Name;

            if (ReferenceEquals(interfaceStruct.unknown, this) && string.Equals(interfaceStruct.iis, iidName, StringComparison.Ordinal))
            {
                return base.QueueInterface(param1, param2);
            }

            IUnknown? result = interfaceStruct.unknown?.QueueInterface(param1, param2);

            if (result != null)
            {
                return result;
            }
        }

        if (param2 == null)
        {
            return null;
        }

        AddQueueeForInterface(param1, param2);

        if (!ReferenceEquals(context, this))
        {
            context.QueueInterface(param1, AnnounceInterfaceAvailability);
        }

        return null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::addQueueeForInterface
    private void AddQueueeForInterface(IID param1, Action<IID, IUnknown?> param2)
    {
        ComponentInterfaceQueue? queue = HasQueueForInterface(param1) ? GetQueueForInterface(param1) : null;

        if (queue == null)
        {
            queue = new ComponentInterfaceQueue(param1);
            _interfacesQueued.Add(queue);
        }

        queue.receivers.Insert(0, param2);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::hasQueueForInterface
    private bool HasQueueForInterface(IID param1)
    {
        string iidName = param1.GetType().FullName ?? param1.GetType().Name;

        foreach (ComponentInterfaceQueue queue in _interfacesQueued)
        {
            if (queue.identifier == null)
            {
                continue;
            }

            string queueIidName = queue.identifier.GetType().FullName ?? queue.identifier.GetType().Name;

            if (string.Equals(queueIidName, iidName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::getQueueForInterface
    private ComponentInterfaceQueue? GetQueueForInterface(IID param1)
    {
        string iidName = param1.GetType().FullName ?? param1.GetType().Name;

        foreach (ComponentInterfaceQueue queue in _interfacesQueued)
        {
            if (queue.identifier == null)
            {
                continue;
            }

            string queueIidName = queue.identifier.GetType().FullName ?? queue.identifier.GetType().Name;

            if (string.Equals(queueIidName, iidName, StringComparison.Ordinal))
            {
                return queue;
            }
        }

        return null;
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::announceInterfaceAvailability
    protected void AnnounceInterfaceAvailability(IID param1, IUnknown? param2)
    {
        ComponentInterfaceQueue? queue = GetQueueForInterface(param1);

        if (queue == null || queue.receivers.Count == 0 || param2 == null)
        {
            return;
        }

        int receiverCount = queue.receivers.Count;

        for (int i = 0;
             i < receiverCount;
             i++)
        {
            IUnknown? interfaceInstance = param2.QueueInterface(param1);

            if (interfaceInstance == null)
            {
                Error("Interface " + (param1.GetType().FullName ?? param1.GetType().Name) + " still unavailable!", true, 6);
            }

            if (queue.receivers.Count <= 0)
            {
                continue;
            }

            Action<IID, IUnknown?> receiver = queue.receivers[^1];

            queue.receivers.RemoveAt(queue.receivers.Count - 1);
            receiver(param1, interfaceInstance);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        base.Dispose();

        while (_attachedComponents.Count > 0)
        {
            Component component = _attachedComponents[^1];

            _attachedComponents.RemoveAt(_attachedComponents.Count - 1);

            if (!ReferenceEquals(component, this))
            {
                component.events.RemoveEventListener(INTERNAL_EVENT_UNLOCKED, UnlockEventHandler);
                component.Dispose();
            }
        }

        while (_interfacesQueued.Count > 0)
        {
            ComponentInterfaceQueue queue = _interfacesQueued[^1];

            _interfacesQueued.RemoveAt(_interfacesQueued.Count - 1);

            queue.Dispose();
        }

        foreach (KeyValuePair<uint, List<IUpdateReceiver>> pair in _updateReceivers)
        {
            pair.Value.Clear();
        }

        _updateReceivers.Clear();
        _linkEventTrackers.Clear();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::registerUpdateReceiver
    public new void RegisterUpdateReceiver(IUpdateReceiver param1, uint param2)
    {
        if (!_updateReceivers.TryGetValue(param2, out List<IUpdateReceiver>? receivers))
        {
            receivers = [];
            _updateReceivers[param2] = receivers;
        }

        if (!receivers.Contains(param1))
        {
            receivers.Add(param1);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::removeUpdateReceiver
    public new void RemoveUpdateReceiver(IUpdateReceiver param1)
    {
        foreach (KeyValuePair<uint, List<IUpdateReceiver>> pair in _updateReceivers)
        {
            pair.Value.Remove(param1);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::update
    public void Update(uint param1)
    {
        foreach (KeyValuePair<uint, List<IUpdateReceiver>> pair in _updateReceivers.OrderBy(param => param.Key))
        {
            foreach (IUpdateReceiver receiver in pair.Value.ToArray())
            {
                if (!receiver.disposed)
                {
                    receiver.Update(param1);
                }
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::toXMLString
    public override string ToXmlString(uint param1 = 0)
    {
        string indent = new('\t', (int)param1);
        StringBuilder builder = new();

        builder.Append(indent).Append("<context class=\"").Append(GetType().FullName).Append("\" >\n");

        InterfaceStructList? ownInterfaces = GetInterfaceStructList(this);
        List<InterfaceStruct> interfaceStructs = new();

        if (ownInterfaces != null)
        {
            ownInterfaces.MapStructsByImplementor(this, interfaceStructs);

            foreach (InterfaceStruct @struct in interfaceStructs)
            {
                builder.Append(indent)
                       .Append("\t<interface iid=\"")
                       .Append(@struct.iis)
                       .Append("\" refs=\"")
                       .Append(@struct.references)
                       .Append("\"/>\n");
            }
        }

        foreach (Component component in _attachedComponents.Where(component => !ReferenceEquals(component, this)))
        {
            builder.Append(component.ToXmlString(param1 + 1));
        }

        builder.Append(indent).Append("</context>\n");
        return builder.ToString();
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::set configuration
    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get configuration
    public ICoreConfiguration? configuration { get; set; }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::addLinkEventTracker
    public void AddLinkEventTracker(ILinkEventTracker param1)
    {
        if (!_linkEventTrackers.Contains(param1))
        {
            _linkEventTrackers.Add(param1);
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::removeLinkEventTracker
    public void RemoveLinkEventTracker(ILinkEventTracker param1)
    {
        _linkEventTrackers.Remove(param1);
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::createLinkEvent
    public void CreateLinkEvent(string param1)
    {
        foreach (ILinkEventTracker tracker in _linkEventTrackers.ToArray())
        {
            if (tracker.linkPattern.Length > 0)
            {
                if (param1.StartsWith(tracker.linkPattern, StringComparison.Ordinal))
                {
                    tracker.LinkReceived(param1);
                }
            }
            else
            {
                tracker.LinkReceived(param1);
            }
        }
    }

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/IContext.as::get linkEventTrackers
    public IList<ILinkEventTracker> linkEventTrackers => _linkEventTrackers;

    /// @see WIN63-202407091256-704579380-Source-main/core/runtime/ComponentContext.as::unlockEventHandler
    private void UnlockEventHandler(object? param1)
    {
        Component? component = param1 switch
        {
            LockEvent lockEvent => lockEvent.unknown as Component,
            Component fallbackComponent => fallbackComponent,
            _ => null,
        };

        if (component == null)
        {
            return;
        }

        if (!component.disposed)
        {
            component.events.RemoveEventListener(INTERNAL_EVENT_UNLOCKED, UnlockEventHandler);
        }

        if (disposed)
        {
            return;
        }

        List<InterfaceStruct> interfaceStructs = new();

        GetInterfaceStructList(this)?.MapStructsByImplementor(component, interfaceStructs);

        while (interfaceStructs.Count > 0 && !component.disposed && !disposed)
        {
            InterfaceStruct interfaceStruct = interfaceStructs[^1];

            interfaceStructs.RemoveAt(interfaceStructs.Count - 1);

            if (interfaceStruct.iid != null)
            {
                AnnounceInterfaceAvailability(interfaceStruct.iid, component);
            }
        }

        root.events.DispatchEvent(COMPONENT_EVENT_UNLOCKED);
    }

    private Component? CreateComponentInstance(Type param1, uint param2, object? param3)
    {
        ConstructorInfo[] constructors = param1.GetConstructors();

        foreach (ConstructorInfo ctor in constructors)
        {
            ParameterInfo[] parameters = ctor.GetParameters();

            try
            {
                switch (parameters.Length)
                {
                    case 3 when
                        typeof(IContext).IsAssignableFrom(parameters[0].ParameterType) &&
                        parameters[1].ParameterType == typeof(uint):
                        return ctor.Invoke([this, (object)param2, param3]) as Component;
                    case 2 when
                        typeof(IContext).IsAssignableFrom(parameters[0].ParameterType) &&
                        parameters[1].ParameterType == typeof(uint):
                        return ctor.Invoke([this, param2]) as Component;
                    case 1 when typeof(IContext).IsAssignableFrom(parameters[0].ParameterType):
                        return ctor.Invoke([this]) as Component;
                    case 0:
                        return ctor.Invoke([]) as Component;
                }
            }
            catch (Exception e)
            {
                Exception inner = e is TargetInvocationException tie ? tie.InnerException ?? e : e;
                Logger.Error(
                    $"[ComponentContext] Constructor {param1.Name}({parameters.Length} params) threw: {inner.GetType().Name}: {inner.Message}",
                    inner
                );
            }
        }

        return null;
    }
}
