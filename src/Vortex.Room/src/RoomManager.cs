using System.Diagnostics;
using System.Xml.Linq;

using Vortex.Core.Runtime;
using Vortex.IID;
using Vortex.Room.Events;
using Vortex.Room.Exceptions;
using Vortex.Room.Object;
using Vortex.Room.Object.Logic;
using Vortex.Room.Object.Visualization;
using Vortex.Room.Object.Visualization.Utils;

namespace Vortex.Room;

/// @see com.sulake.room.RoomManager
public class RoomManager : Component, IRoomManager, IRoomInstanceContainer
{
    public const int ROOM_MANAGER_ERROR = -1;
    public const int ROOM_MANAGER_LOADING = 0;
    public const int ROOM_MANAGER_LOADED = 1;
    public const int ROOM_MANAGER_INITIALIZING = 2;
    public const int ROOM_MANAGER_INITIALIZED = 3;

    private const int CONTENT_PROCESSING_TIME_LIMIT_MILLISECONDS = 40;

    private Dictionary<string, RoomInstance>? _rooms;
    private IRoomContentLoader? _contentLoader;
    private List<string> _pendingContentTypes;
    private List<int> _updateCategories;
    private int _placeholderExpectedCount;
    private IRoomManagerListener? _listener;
    private IRoomObjectFactory? _objectFactory;
    private IRoomObjectVisualizationFactory? _visualizationFactory;
    private int _state;
    private XElement? _pendingInitXml;
    private readonly List<string> _loadedContentQueue;
    private bool _skipContentProcessingForNextFrame;
    private bool _limitContentProcessing = true;

    public RoomManager(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
        _loadedContentQueue = [];
        _rooms = new Dictionary<string, RoomInstance>();
        _pendingContentTypes = [];
        _updateCategories = [];
        events.AddEventListener(RoomContentLoadedEvent.CONTENT_LOAD_SUCCESS, OnContentLoaded);
        events.AddEventListener(RoomContentLoadedEvent.CONTENT_LOAD_FAILURE, OnContentLoaded);
        events.AddEventListener(RoomContentLoadedEvent.CONTENT_LOAD_CANCEL, OnContentLoaded);
    }

    public new bool disposed { get; private set; }

    public bool LimitContentProcessing
    {
        set => _limitContentProcessing = value;
    }

    protected override IList<ComponentDependency> dependencies
    {
        get
        {
            List<ComponentDependency> baseDeps = new(base.dependencies)
            {
                new(
                    new IIDRoomObjectFactory(),
                    value => _objectFactory = value as IRoomObjectFactory
                ),
                new(
                    new IIDRoomObjectVisualizationFactory(),
                    value => _visualizationFactory = value as IRoomObjectVisualizationFactory
                ),
            };
            return baseDeps;
        }
    }

    protected override void InitComponent()
    {
        _state = ROOM_MANAGER_LOADED;

        if (_pendingInitXml == null)
        {
            return;
        }

        XElement? xml = _pendingInitXml;
        _pendingInitXml = null;

        Initialize(xml, _listener!);
    }

    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }
        if (_rooms != null)
        {
            foreach (RoomInstance room in _rooms.Values)
            {
                room.Dispose();
            }
            _rooms.Clear();
            _rooms = null;
        }
        _listener = null;
        _pendingContentTypes = null!;
        _updateCategories = null!;
        _contentLoader = null;
        disposed = true;
        base.Dispose();
    }

    public bool Initialize(XElement? xml, IRoomManagerListener listener)
    {
        switch (_state)
        {
            case ROOM_MANAGER_LOADING when _pendingInitXml != null:
                return false;
            case ROOM_MANAGER_LOADING:
                _pendingInitXml = xml;
                _listener = listener;
                return true;
            case >= ROOM_MANAGER_INITIALIZING:
                return false;
        }

        if (xml == null)
        {
            return false;
        }

        if (_contentLoader == null)
        {
            return false;
        }

        _placeholderExpectedCount = 50;
        _listener = listener;

        string[]? placeHolderTypes = _contentLoader.GetPlaceHolderTypes();

        if (placeHolderTypes != null)
        {
            foreach (string type in placeHolderTypes)
            {
                if (_pendingContentTypes.Contains(type))
                {
                    continue;
                }

                _contentLoader.LoadObjectContent(type, events);
                _pendingContentTypes.Add(type);
            }
        }

        _state = ROOM_MANAGER_INITIALIZING;

        return true;
    }

    public void SetContentLoader(IRoomContentLoader loader)
    {
        _contentLoader?.Dispose();
        _contentLoader = loader;
    }

    public void AddObjectUpdateCategory(int category)
    {
        if (_updateCategories.Contains(category))
        {
            return;
        }

        _updateCategories.Add(category);

        if (_rooms == null)
        {
            return;
        }

        foreach (RoomInstance room in _rooms.Values)
        {
            room.AddObjectUpdateCategory(category);
        }
    }

    public void RemoveObjectUpdateCategory(int category)
    {
        if (!_updateCategories.Remove(category))
        {
            return;
        }

        if (_rooms == null)
        {
            return;
        }

        foreach (RoomInstance room in _rooms.Values)
        {
            room.RemoveObjectUpdateCategory(category);
        }
    }

    public IRoomInstance? CreateRoom(string id, XElement? xml)
    {
        if (_state < ROOM_MANAGER_INITIALIZED)
        {
            throw new RoomManagerException("RoomManager not initialized");
        }

        if (_rooms!.ContainsKey(id))
        {
            return null;
        }

        RoomInstance room = new(id, this);
        _rooms[id] = room;

        for (int i = _updateCategories.Count - 1;
             i >= 0;
             i--)
        {
            room.AddObjectUpdateCategory(_updateCategories[i]);
        }

        return room;
    }

    public IRoomInstance? GetRoom(string id)
    {
        if (_rooms != null && _rooms.TryGetValue(id, out RoomInstance? room))
        {
            return room;
        }

        return null;
    }

    public IRoomInstance? GetRoomWithIndex(int index)
    {
        if (_rooms == null || index < 0 || index >= _rooms.Count)
        {
            return null;
        }

        int i = 0;

        foreach (RoomInstance room in _rooms.Values)
        {
            if (i == index)
            {
                return room;
            }
            i++;
        }

        return null;
    }

    public int RoomCount => _rooms?.Count ?? 0;

    public bool DisposeRoom(string id)
    {
        if (_rooms == null || !_rooms.Remove(id, out RoomInstance? room))
        {
            return false;
        }

        room.Dispose();

        return true;

    }

    public IRoomObject? CreateRoomObject(string roomId, int objectId, string type, int category)
    {
        if (_state < ROOM_MANAGER_INITIALIZED)
        {
            throw new RoomManagerException("RoomManager not initialized");
        }

        IRoomInstance? instance = GetRoom(roomId);

        if (instance == null || _contentLoader == null)
        {
            return null;
        }

        if (instance is not RoomInstance room)
        {
            return null;
        }

        IGraphicAssetCollection? assetCollection = null;
        XElement? visualizationXml = null;
        XElement? logicXml = null;
        string? visualizationType;
        string? logicType;
        string contentType = type;
        bool useplaceholder = false;

        if (!_contentLoader.HasInternalContent(type))
        {
            assetCollection = _contentLoader.GetGraphicAssetCollection(type);

            if (assetCollection == null)
            {
                useplaceholder = true;
                _contentLoader.LoadObjectContent(type, events);
                contentType = _contentLoader.GetPlaceHolderType(type) ?? type;
                assetCollection = _contentLoader.GetGraphicAssetCollection(contentType);
            }

            visualizationXml = _contentLoader.GetVisualizationXml(contentType);
            logicXml = _contentLoader.GetLogicXml(contentType);

            if (visualizationXml == null || assetCollection == null)
            {
                return null;
            }

            visualizationType = _contentLoader.GetVisualizationType(contentType);
            logicType = _contentLoader.GetLogicType(contentType);
        }
        else
        {
            visualizationType = type;
            logicType = type;
        }

        int stateCount = 1;
        IRoomObject? obj = room.CreateObjectInternal(objectId, stateCount, type, category);

        if (obj is not IRoomObjectController controller)
        {
            return null;
        }

        if (_visualizationFactory == null)
        {
            instance.DisposeObject(objectId, category);
            return null;
        }

        IRoomObjectGraphicVisualization? visualization = _visualizationFactory.CreateRoomObjectVisualization(visualizationType!);

        if (visualization == null)
        {
            instance.DisposeObject(objectId, category);

            return null;
        }

        visualization.AssetCollection = assetCollection;

        string imageUrlBase = context.configuration.GetProperty("stories.image_url_base");
        string extraDataUrl = context.configuration.GetProperty("extra_data_service_url");
        bool extraDataBatches = context.configuration.GetBoolean("extra_data_batches_enabled");

        visualization.SetExternalBaseUrls(imageUrlBase, extraDataUrl, extraDataBatches);

        IRoomObjectVisualizationData? visualizationData =
            _visualizationFactory.GetRoomObjectVisualizationData(contentType!, visualizationType!, visualizationXml);

        if (!visualization.Initialize(visualizationData!))
        {
            instance.DisposeObject(objectId, category);

            return null;
        }

        controller.SetVisualization(visualization);

        IRoomObjectEventHandler? eventHandler = _objectFactory?.CreateRoomObjectLogic(logicType!);

        controller.SetEventHandler(eventHandler!);

        if (eventHandler != null && logicXml != null)
        {
            eventHandler.Initialize(logicXml);
        }

        if (!useplaceholder)
        {
            controller.SetInitialized(true);
        }

        _contentLoader.RoomObjectCreated(controller, roomId);

        return controller;
    }

    public IRoomObjectManager CreateRoomObjectManager()
    {
        return _objectFactory != null
            ? _objectFactory.CreateRoomObjectManager()
            : new RoomObjectManager();
    }

    public bool IsContentAvailable(string type)
    {
        return _contentLoader?.GetGraphicAssetCollection(type) != null;
    }

    private void ProcessInitialContentLoad(string? type)
    {
        if (type == null || _state == ROOM_MANAGER_ERROR || _contentLoader == null)
        {
            if (_contentLoader == null)
            {
                _state = ROOM_MANAGER_ERROR;
            }

            return;
        }

        if (_contentLoader.GetGraphicAssetCollection(type) != null)
        {
            _pendingContentTypes.Remove(type);

            if (_pendingContentTypes.Count != 0)
            {
                return;
            }

            _state = ROOM_MANAGER_INITIALIZED;
            _listener?.RoomManagerInitialized(true);
        }
        else
        {
            _state = ROOM_MANAGER_ERROR;
            _listener?.RoomManagerInitialized(false);
        }
    }

    private void OnContentLoaded(object? args)
    {
        if (_contentLoader == null)
        {
            return;
        }

        RoomContentLoadedEvent? evt = args as RoomContentLoadedEvent;
        string? contentType = evt?.ContentType;

        if (contentType == null)
        {
            _listener?.ContentLoaded(null!, false);

            return;
        }

        if (!_loadedContentQueue.Contains(contentType))
        {
            _loadedContentQueue.Add(contentType);
        }
    }

    private void ProcessLoadedContentTypes()
    {
        if (_skipContentProcessingForNextFrame)
        {
            _skipContentProcessingForNextFrame = false;
            return;
        }

        Stopwatch sw = Stopwatch.StartNew();

        while (_loadedContentQueue.Count > 0)
        {
            string contentType = _loadedContentQueue[0];
            _loadedContentQueue.RemoveAt(0);

            if (!_contentLoader!.HasVisualizationXml(contentType))
            {
                _listener?.ContentLoaded(contentType, false);

                return;
            }

            IGraphicAssetCollection? collection = _contentLoader.GetGraphicAssetCollection(contentType);

            if (collection == null)
            {
                _listener?.ContentLoaded(contentType, false);

                return;
            }

            UpdateObjectContents(contentType);
            _listener?.ContentLoaded(contentType, true);

            if (_pendingContentTypes.Count > 0)
            {
                ProcessInitialContentLoad(contentType);
            }

            if (sw.ElapsedMilliseconds < CONTENT_PROCESSING_TIME_LIMIT_MILLISECONDS || !_limitContentProcessing)
            {
                continue;
            }

            _skipContentProcessingForNextFrame = true;

            break;
        }
    }

    private void UpdateObjectContents(string? type)
    {
        if (type == null || _contentLoader == null || _visualizationFactory == null)
        {
            return;
        }

        string? visualizationType = _contentLoader.GetVisualizationType(type);
        string? logicType = _contentLoader.GetLogicType(type);
        IRoomObjectVisualizationData? visualizationData = null;

        if (_rooms == null)
        {
            return;
        }

        foreach ((string roomId, RoomInstance room) in _rooms)
        {
            int[] managerIds = room.GetObjectManagerIds();
            bool anyInitialized = false;

            foreach (int managerId in managerIds)
            {
                int count = room.GetObjectCountForType(type, managerId);
                for (int i = count - 1;
                     i >= 0;
                     i--)
                {
                    if (room.GetObjectWithIndexAndType(i, type, managerId) is not IRoomObjectController controller)
                    {
                        continue;
                    }

                    if (visualizationData == null)
                    {
                        XElement? visXml = _contentLoader.GetVisualizationXml(type);

                        if (visXml == null)
                        {
                            return;
                        }

                        XElement? logicXml = _contentLoader.GetLogicXml(type);
                        IGraphicAssetCollection? assetCollection = _contentLoader.GetGraphicAssetCollection(type);

                        if (assetCollection == null)
                        {
                            return;
                        }
                        visualizationData = _visualizationFactory.GetRoomObjectVisualizationData(type, visualizationType!, visXml);
                    }

                    IRoomObjectGraphicVisualization?
                        visualization = _visualizationFactory.CreateRoomObjectVisualization(visualizationType!);

                    if (visualization == null)
                    {
                        room.DisposeObject(controller.Id, managerId);

                        continue;
                    }

                    IGraphicAssetCollection? assetCol = _contentLoader.GetGraphicAssetCollection(type);
                    visualization.AssetCollection = assetCol;

                    string imageUrlBase = context.configuration.GetProperty("stories.image_url_base");
                    string extraDataUrl = context.configuration.GetProperty("extra_data_service_url");
                    bool extraDataBatches = context.configuration.GetBoolean("extra_data_batches_enabled");

                    visualization.SetExternalBaseUrls(imageUrlBase, extraDataUrl, extraDataBatches);

                    if (!visualization.Initialize(visualizationData!))
                    {
                        room.DisposeObject(controller.Id, managerId);

                        continue;
                    }

                    controller.SetVisualization(visualization);

                    IRoomObjectEventHandler? eventHandler = _objectFactory?.CreateRoomObjectLogic(logicType!);

                    controller.SetEventHandler(eventHandler!);

                    if (eventHandler != null)
                    {
                        XElement? logXml = _contentLoader.GetLogicXml(type);

                        eventHandler.Initialize(logXml);
                    }

                    controller.SetInitialized(true);
                    _listener?.ObjectInitialized(roomId, controller.Id, managerId);
                    anyInitialized = true;
                }
            }

            if (!room.HasUninitializedObjects() && anyInitialized)
            {
                _listener?.ObjectsInitialized(roomId);
            }
        }
    }

    public void Update(uint time)
    {
        ProcessLoadedContentTypes();

        if (_rooms == null)
        {
            return;
        }

        foreach (RoomInstance room in _rooms.Values)
        {
            room.Update();
        }
    }
}
