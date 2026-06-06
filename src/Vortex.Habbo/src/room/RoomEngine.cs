using System;
using System.Xml.Linq;

using Godot;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Room;
using Vortex.Room.Object;
using Vortex.Room.Renderer;
using Vortex.Room.Utils;
using Vortex.Habbo.Communication;
using Vortex.Habbo.Configuration;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Habbo.Room.Object;
using Vortex.Habbo.Room.Object.Data;
using Vortex.Habbo.Room.Utils;
using Vortex.Habbo.Session;
using Vortex.Habbo.Toolbar;
using Vortex.Habbo.Window;

using RoomObjectUpdateMessage = Vortex.Room.Messages.RoomObjectUpdateMessage;
using IRoomObjectVisualizationFactory = Vortex.Room.Object.IRoomObjectVisualizationFactory;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.RoomEngine
public class RoomEngine : Component,
    IRoomEngine,
    IRoomManagerListener,
    IRoomCreator,
    IRoomEngineServices,
    IUpdateReceiver,
    IRoomContentListener
{
    public const uint SETUP_WITHOUT_TOOLBAR = 1;
    public const uint SETUP_WITHOUT_COMMUNICATION = 2;
    public const uint SETUP_WITHOUT_GAME_MANAGER = 4;
    public const uint SETUP_WITHOUT_COMMUNICATION_AND_TOOLBAR = 5;

    private const string ROOM_TEMP_ID = "temporary_room";
    public const int OBJECT_ID_ROOM = -1;
    private const string OBJECT_TYPE_ROOM = "room";
    private const int OBJECT_ID_ROOM_HIGHLIGHTER = -2;
    private const string OBJECT_TYPE_ROOM_HIGHLIGHTER = "tile_cursor";
    private const int OBJECT_ID_SELECTION_ARROW = -3;
    private const string OBJECT_TYPE_SELECTION_ARROW = "selection_arrow";
    private const string OBJECT_TYPE_OVERLAY = "overlay";
    private const string OBJECT_ICON_SPRITE = "object_icon_sprite";
    private const int ROOM_DRAG_THRESHOLD = 15;
    private const int FURNITURE_CREATION_TIME_BUDGET_MS = 40;

    protected int _activeRoomId;
    private IHabboCommunicationManager? _communication;
    private IHabboConfigurationManager? _configurationManager;
    private IRoomRendererFactory? _roomRendererFactory;
    private IRoomObjectFactory? _roomObjectFactory;
    private IRoomObjectVisualizationFactory? _visualizationFactory;
    // TODO: IHabboAdManager _adManager — unported
    private RoomObjectEventHandler? _eventHandler;
    private RoomMessageHandler? _messageHandler;
    private RoomContentLoader? _roomContentLoader;
    private bool _contentLoaderReady;
    private NumberBank? _imageObjectIdBank;
    private Dictionary<int, object>? _imageCallbackListeners;
    private NumberBank? _thumbnailObjectIdBank;
    private Dictionary<int, object>? _thumbnailCallbackInfo;
    private int _activeCanvasId = -1;
    private int _lastMouseX;
    private int _lastMouseY;
    private bool _isMouseDown;
    private bool _isDragActive;
    private int _dragStartX;
    private int _dragStartY;
    private bool _roomDraggingAlwaysCenters;
    private Dictionary<string, XElement>? _pendingRoomInitData;
    private Dictionary<string, RoomInstanceData>? _roomInstanceData;
    private bool _skipFurnitureCreationForNextFrame;
    private bool _mouseCursorUpdate;
    private Dictionary<string, object>? _badgeRequestMap;
    // TODO: IHabboGameManager _gameManager — unported
    private int _savedMouseEventsDisabledAboveY;
    private int _savedMouseEventsDisabledLeftToX;
    private bool _whereYouClickIsWhereYouGo = true;
    private bool _isMoveBlocked;
    // TODO: RoomAreaSelectionManager _areaSelectionManager — unported

    private IRoomManager? _roomManager;
    private ISessionDataManager? _sessionDataManager;
    private IRoomSessionManager? _roomSessionManager;
    private IHabboToolbar? _toolbar;
    // TODO: IHabboCatalog _catalog — unported
    private IHabboWindowManager? _windowManager;

    /// @see com.sulake.habbo.room.RoomEngine::RoomEngine
    public RoomEngine(IContext? param1 = null, uint param2 = 0, object? param3 = null)
        : base(param1, param2, param3)
    {
    }

    /// @see com.sulake.habbo.room.RoomEngine::get dependencies
    protected override IList<ComponentDependency> dependencies
    {
        get
        {
            List<ComponentDependency> deps =
            [
                // 1. IRoomObjectFactory (required)
                new ComponentDependency(
                    new IID.IIDRoomObjectFactory(), o => _roomObjectFactory = o as IRoomObjectFactory
                ),
                // 2. IRoomObjectVisualizationFactory (required)
                new ComponentDependency(
                    new IID.IIDRoomObjectVisualizationFactory(), o => _visualizationFactory = o as IRoomObjectVisualizationFactory
                ),
                // 3. IRoomManager (required)
                new ComponentDependency(
                    new IID.IIDRoomManager(), o => _roomManager = o as IRoomManager
                ),
                // 4. IRoomRendererFactory (required)
                new ComponentDependency(
                    new IID.IIDRoomRendererFactory(), o => _roomRendererFactory = o as IRoomRendererFactory
                ),
                // 5. IHabboCommunicationManager (conditional on flags)
                new ComponentDependency(
                    new IID.IIDHabboCommunicationManager(),
                    o => _communication = o as IHabboCommunicationManager,
                    (_flags & SETUP_WITHOUT_COMMUNICATION_AND_TOOLBAR) == 0
                ),
                // 6. IHabboConfigurationManager (required, event: "complete" -> OnConfigurationComplete)
                new ComponentDependency(
                    new IID.IIDHabboConfigurationManager(),
                    o => _configurationManager = o as IHabboConfigurationManager,
                    true,
                    [new DependencyEventListener("complete", _ => OnConfigurationComplete())]
                ),
                // 7. IHabboAdManager (optional, 3 ad events) — TODO: wire when ad manager is ported
                // new ComponentDependency(
                //     new IID.IIDHabboAdManager(), ...
                // ),
                // 8. ISessionDataManager — TODO: wire when session data manager is ported
                new ComponentDependency(
                    new IID.IIDSessionDataManager(), o => _sessionDataManager = o as ISessionDataManager
                ),
                // 9. IRoomSessionManager (optional, 2 session events) — TODO: wire when session manager is ported
                new ComponentDependency(
                    new IID.IIDHabboRoomSessionManager(),
                    o => _roomSessionManager = o as IRoomSessionManager,
                    false
                ),
                // 10. IHabboToolbar (optional)
                new ComponentDependency(
                    new IID.IIDHabboToolbar(), o => _toolbar = o as IHabboToolbar, false
                ),
                // 11. IHabboCatalog (optional) — TODO: wire when catalog is ported
                // new ComponentDependency(
                //     new IID.IIDHabboCatalog(), o => _catalog = o as IHabboCatalog, false
                // ),
                // 12. IHabboGameManager (conditional on flags) — TODO: wire when game manager is ported
                // new ComponentDependency(
                //     new IID.IIDHabboGameManager(), ...
                // ),
                // 13. IHabboWindowManager — TODO: wire when window manager dep is needed
                new ComponentDependency(
                    new IID.IIDHabboWindowManager(), o => _windowManager = o as IHabboWindowManager
                ),
            ];

            return deps;
        }
    }

    #region Properties (IRoomEngine)
    /// @see com.sulake.habbo.room.RoomEngine::get events
    object? IRoomEngine.Events => events;

    /// @see com.sulake.habbo.room.RoomEngine::get isInitialized
    public bool IsInitialized { get; private set; }

    /// @see com.sulake.habbo.room.RoomEngine::get activeRoomId
    public int ActiveRoomId => _activeRoomId;

    /// @see com.sulake.habbo.room.RoomEngine::get isDecorateMode
    public bool IsDecorateMode
    {
        get
        {
            RoomInstanceData? data = GetRoomInstanceData(_activeRoomId);
            return data?.IsDecorateMode ?? false;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::get isGameMode
    public bool IsGameMode { get; set; }

    /// @see com.sulake.habbo.room.RoomEngine::set disableUpdate
    public bool DisableUpdate { set; private get; }

    /// @see com.sulake.habbo.room.RoomEngine::get mouseEventsDisabledAboveY
    public int MouseEventsDisabledAboveY { get; set; }

    /// @see com.sulake.habbo.room.RoomEngine::get mouseEventsDisabledLeftToX
    public int MouseEventsDisabledLeftToX { get; set; }

    /// @see com.sulake.habbo.room.RoomEngine::get activeRoomHasHanditemControlBlocked
    public bool ActiveRoomHasHanditemControlBlocked
    {
        get
        {
            RoomInstanceData? data = GetRoomInstanceData(_activeRoomId);
            return data?.HanditemControlBlocked ?? false;
        }
    }
    #endregion

    #region Properties (IRoomEngineServices)
    /// @see com.sulake.habbo.room.RoomEngine::get connection
    public IConnection? Connection => _communication?.connection;

    /// @see com.sulake.habbo.room.RoomEngine::get events (IRoomEngineServices)
    object? IRoomEngineServices.Events => events;

    /// @see com.sulake.habbo.room.RoomEngine::get configuration
    public ICoreConfiguration? Configuration => this;

    /// @see com.sulake.habbo.room.RoomEngine::get playerUnderCursor
    public int PlayerUnderCursor { get; private set; } = -1;

    bool IRoomEngineServices.ActiveRoomHasHanditemControlBlocked => ActiveRoomHasHanditemControlBlocked;
    bool IRoomEngineServices.IsDecorateMode => IsDecorateMode;
    bool IRoomEngineServices.IsGameMode => IsGameMode;
    #endregion

    #region Lifecycle
    /// @see com.sulake.habbo.room.RoomEngine::initComponent
    protected override void InitComponent()
    {
        _roomInstanceData = new Dictionary<string, RoomInstanceData>(StringComparer.Ordinal);
        _imageObjectIdBank = new NumberBank(1000);
        _thumbnailObjectIdBank = new NumberBank(1000);
        _imageCallbackListeners = new Dictionary<int, object>();
        _thumbnailCallbackInfo = new Dictionary<int, object>();
        _pendingRoomInitData = new Dictionary<string, XElement>(StringComparer.Ordinal);
        _eventHandler = new RoomObjectEventHandler(this);
        _messageHandler = new RoomMessageHandler(this);
        RegisterUpdateReceiver(this, 1);
        _roomObjectFactory?.AddObjectEventListener(OnRoomObjectEvent);
        // TODO: _areaSelectionManager = new RoomAreaSelectionManager(this);

        // Config manager may have already fired "complete" before our listener was registered.
        if (_configurationManager?.IsInitialized() == true)
        {
            OnConfigurationComplete();
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::dispose
    public override void Dispose()
    {
        if (disposed)
        {
            return;
        }

        RemoveUpdateReceiver(this);

        // TODO: Dispose _areaSelectionManager

        _imageObjectIdBank?.Dispose();
        _imageObjectIdBank = null;

        _thumbnailObjectIdBank?.Dispose();
        _thumbnailObjectIdBank = null;

        _imageCallbackListeners?.Clear();
        _imageCallbackListeners = null;

        _thumbnailCallbackInfo?.Clear();
        _thumbnailCallbackInfo = null;

        if (_eventHandler != null)
        {
            _eventHandler.Dispose();
            _eventHandler = null;
        }

        if (_messageHandler != null)
        {
            _messageHandler.Dispose();
            _messageHandler = null;
        }

        if (_roomContentLoader != null)
        {
            _roomContentLoader.Dispose();
            _roomContentLoader = null;
        }

        _pendingRoomInitData?.Clear();
        _pendingRoomInitData = null;

        if (_roomInstanceData != null)
        {
            foreach (RoomInstanceData instanceData in _roomInstanceData.Values)
            {
                instanceData.Dispose();
            }

            _roomInstanceData.Clear();
            _roomInstanceData = null;
        }

        _badgeRequestMap?.Clear();
        _badgeRequestMap = null;

        base.Dispose();
    }

    /// @see com.sulake.habbo.room.RoomEngine::update
    public void Update(uint param1)
    {
        if (disposed || DisableUpdate)
        {
            return;
        }

        if (_roomManager != null)
        {
            _roomManager.Update(param1);
        }

        UpdateRoomCameras(param1);
        CreateRoomFurniture(param1);

        _mouseCursorUpdate = false;
    }
    #endregion

    #region Configuration
    /// @see com.sulake.habbo.room.RoomEngine::onConfigurationComplete
    private void OnConfigurationComplete()
    {
        if (_roomContentLoader != null)
        {
            _roomContentLoader.Dispose();
            events.RemoveEventListener(RoomContentLoader.CONTENT_LOADER_READY, OnContentLoaderReady);
        }

        _roomContentLoader = new RoomContentLoader("");
        _roomContentLoader.IconAssets = assets as Core.Assets.IAssetLibrary;
        _roomContentLoader.IconListener = this;
        _roomContentLoader.VisualizationFactory = _visualizationFactory;

        if (_roomManager != null)
        {
            _roomManager.AddObjectUpdateCategory(10);
            _roomManager.AddObjectUpdateCategory(20);
            _roomManager.AddObjectUpdateCategory(100);
            _roomManager.AddObjectUpdateCategory(200);
            _roomManager.AddObjectUpdateCategory(0);
            _roomManager.SetContentLoader(_roomContentLoader);
        }

        if (_messageHandler != null && _communication != null)
        {
            _messageHandler.Connection = _communication.connection;
        }

        _roomDraggingAlwaysCenters = GetBoolean("room.dragging.always_center");

        _roomContentLoader.SessionDataManager = _sessionDataManager;

        // The current furni-data adaptation can complete synchronously during Initialize().
        events.AddEventListener(RoomContentLoader.CONTENT_LOADER_READY, OnContentLoaderReady);
        _roomContentLoader.Initialize(events, this);
    }

    /// @see com.sulake.habbo.room.RoomEngine::onContentLoaderReady
    private void OnContentLoaderReady(object? data)
    {
        _contentLoaderReady = true;
        events.RemoveEventListener(RoomContentLoader.CONTENT_LOADER_READY, OnContentLoaderReady);
        events.DispatchEvent(new RoomEngineEvent(RoomEngineEvent.ROOM_ENGINE_INITIALIZED, 0));
        IsInitialized = true;

        if (_pendingRoomInitData != null && _pendingRoomInitData.Count > 0)
        {
            foreach (KeyValuePair<string, XElement> kvp in _pendingRoomInitData)
            {
                if (int.TryParse(kvp.Key, out int roomId))
                {
                    InitializeRoom(roomId, kvp.Value);
                }
            }

            _pendingRoomInitData.Clear();
        }
    }
    #endregion

    #region Room Instance Management
    /// @see com.sulake.habbo.room.RoomEngine::getRoomIdentifier
    public static string GetRoomIdentifier(int roomId)
    {
        return roomId.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomIdFromString
    public static int GetRoomIdFromString(string roomIdStr)
    {
        return int.TryParse(roomIdStr, out int id) ? id : 0;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomInstanceData
    private RoomInstanceData? GetRoomInstanceData(int roomId)
    {
        string roomIdStr = GetRoomIdentifier(roomId);

        if (_roomInstanceData != null && _roomInstanceData.TryGetValue(roomIdStr, out RoomInstanceData? data))
        {
            return data;
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoom
    public IRoomInstance? GetRoom(int roomId)
    {
        return _roomManager?.GetRoom(GetRoomIdentifier(roomId));
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectCategory
    public int GetRoomObjectCategory(string type)
    {
        return _roomContentLoader?.GetObjectCategory(type) ?? -2;
    }

    /// @see com.sulake.habbo.room.RoomEngine::initializeRoom
    public void InitializeRoom(int roomId, XElement? xml)
    {
        if (!_contentLoaderReady)
        {
            if (xml != null)
            {
                _pendingRoomInitData![GetRoomIdentifier(roomId)] = xml;
            }

            return;
        }

        string roomIdStr = GetRoomIdentifier(roomId);

        if (_roomManager?.GetRoom(roomIdStr) != null)
        {
            return;
        }

        _roomManager?.CreateRoom(roomIdStr, xml);
    }

    /// @see com.sulake.habbo.room.RoomEngine::disposeRoom
    public void DisposeRoom(int roomId)
    {
        string roomIdStr = GetRoomIdentifier(roomId);
        _roomManager?.DisposeRoom(roomIdStr);

        if (_roomInstanceData != null && _roomInstanceData.Remove(roomIdStr, out RoomInstanceData? data))
        {
            data.Dispose();
        }

        if (_roomInstanceData == null || _roomInstanceData.Count == 0)
        {
            _activeRoomId = 0;
            _toolbar?.SetToolbarState(HabboToolbarEnum.TOOLBAR_STATE_HOTEL_VIEW);
        }

        events.DispatchEvent(new RoomEngineEvent(RoomEngineEvent.ROOM_DISPOSED, roomId));
    }

    /// @see com.sulake.habbo.room.RoomEngine::setActiveRoom
    public void SetActiveRoom(int roomId)
    {
        if (roomId == _activeRoomId)
        {
            return;
        }

        _activeRoomId = roomId;
        _toolbar?.SetToolbarState(HabboToolbarEnum.TOOLBAR_STATE_ROOM_VIEW);
    }

    /// @see com.sulake.habbo.room.RoomEngine::setOwnUserId
    public void SetOwnUserId(int roomId, int userId)
    {
        if (_roomSessionManager?.GetSession(roomId) is IRoomSession roomSession)
        {
            roomSession.ownUserRoomId = userId;
        }
        RoomCamera? camera = GetRoomCamera(roomId);

        if (camera != null)
        {
            camera.TargetId = userId;
            camera.TargetCategory = RoomObjectCategoryEnum.OBJECT_CATEGORY_USER;
            camera.ActivateFollowing(CameraFollowDuration);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::setWorldType
    public void SetWorldType(int roomId, string? type)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);

        if (data != null)
        {
            data.WorldType = type;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getWorldType
    public string? GetWorldType(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.WorldType;
    }
    #endregion

    #region Room Object Access
    /// @see com.sulake.habbo.room.RoomEngine::getRoomObject
    public IRoomObject? GetRoomObject(int roomId, int objectId, int category)
    {
        IRoomInstance? room = GetRoom(roomId);
        IRoomObject? obj = room?.GetObject(objectId, category);

        if (obj == null && category is 10 or 20)
        {
            obj = CreateObjectFromData(roomId, objectId, category);
        }

        return obj;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectWithIndex
    public IRoomObject? GetRoomObjectWithIndex(int roomId, int index, int category)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.GetObjectWithIndex(index, category);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectCount
    public int GetRoomObjectCount(int roomId, int category)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.GetObjectCount(category) ?? 0;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getObjectsByCategory
    public List<IRoomObject>? GetObjectsByCategory(int category)
    {
        IRoomInstance? room = GetRoom(_activeRoomId);

        if (room == null)
        {
            return null;
        }

        int count = room.GetObjectCount(category);
        List<IRoomObject> result = new(count);

        for (int i = 0; i < count; i++)
        {
            IRoomObject? obj = room.GetObjectWithIndex(i, category);

            if (obj != null)
            {
                result.Add(obj);
            }
        }

        return result;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getObjectRoom
    public IRoomObjectController? GetObjectRoom(int roomId)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.GetObject(OBJECT_ID_ROOM, 0) as IRoomObjectController;
    }
    #endregion

    #region Private Helpers
    /// @see com.sulake.habbo.room.RoomEngine::roomObjectEventHandler
    private void OnRoomObjectEvent(object? data)
    {
        if (_eventHandler != null && data is Vortex.Room.Events.RoomObjectEvent roomEvent)
        {
            _eventHandler.HandleRoomObjectEvent(roomEvent, _activeRoomId);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getObjectFurniture
    private IRoomObjectController? GetObjectFurniture(int roomId, int objectId)
    {
        return GetRoom(roomId)?.GetObject(objectId, RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getObjectWallItem
    private IRoomObjectController? GetObjectWallItem(int roomId, int objectId)
    {
        return GetRoom(roomId)?.GetObject(objectId, RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getObjectUser
    private IRoomObjectController? GetObjectUser(int roomId, int objectId)
    {
        return GetRoom(roomId)?.GetObject(objectId, RoomObjectCategoryEnum.OBJECT_CATEGORY_USER) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::createObjectFurniture
    private IRoomObjectController? CreateObjectFurniture(int roomId, int objectId, string type)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.CreateRoomObject(objectId, type, RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::createObjectWallItem
    private IRoomObjectController? CreateObjectWallItem(int roomId, int objectId, string type)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.CreateRoomObject(objectId, type, RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::createObjectUser
    private IRoomObjectController? CreateObjectUser(int roomId, int objectId, string type)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.CreateRoomObject(objectId, type, RoomObjectCategoryEnum.OBJECT_CATEGORY_USER) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::createObjectSnowWar
    private IRoomObjectController? CreateObjectSnowWar(int roomId, int objectId, string type, int category)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.CreateRoomObject(objectId, type, category) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomCanvas
    private IRoomRenderingCanvas? GetRoomCanvas(int roomId, int canvasId)
    {
        IRoomInstance? room = GetRoom(roomId);
        IRoomRenderer? renderer = room?.GetRenderer() as IRoomRenderer;
        return renderer?.GetCanvas(canvasId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::fixedUserLocation
    private IVector3d? FixedUserLocation(int roomId, IVector3d? location)
    {
        if (location == null)
        {
            return null;
        }

        double floorY = GetRoomNumberValue(roomId, "room_floor_y");
        return new Vector3d(location.X, location.Y, location.Z + floorY);
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectToTileMap
    private void AddObjectToTileMap(int roomId, IRoomObjectController obj)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        TileObjectMap? tileMap = data?.TileObjectMap;
        tileMap?.AddRoomObject(obj);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomCamera
    private RoomCamera? GetRoomCamera(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.RoomCamera;
    }

    /// @see com.sulake.habbo.room.RoomEngine::disposeObject
    private void DisposeObject(int roomId, int objectId, int category)
    {
        IRoomInstance? room = GetRoom(roomId);
        room?.DisposeObject(objectId, category);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getFurnitureColorIndex
    private int GetFurnitureColorIndex(int typeId)
    {
        return _roomContentLoader?.GetActiveObjectColorIndex(typeId) ?? -1;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getWallItemColorIndex
    private int GetWallItemColorIndex(int typeId)
    {
        return _roomContentLoader?.GetWallItemColorIndex(typeId) ?? -1;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectAdURL
    private string GetRoomObjectAdUrl(string? type)
    {
        return _roomContentLoader?.GetRoomObjectAdURL(type ?? "") ?? "";
    }

    /// @see com.sulake.habbo.room.RoomEngine::handleRoomDragging
    private bool HandleRoomDragging(IRoomRenderingCanvas canvas, int x, int y, string type,
        bool altKey, bool ctrlKey, bool shiftKey)
    {
        if (type == "mouseDown")
        {
            _isMouseDown = true;
            _dragStartX = x;
            _dragStartY = y;
            _isDragActive = false;
        }
        else if (type == "mouseUp" || type == "click")
        {
            bool wasDragging = _isDragActive;
            _isMouseDown = false;
            _isDragActive = false;

            if (wasDragging)
            {
                return true;
            }
        }
        else if (type == "mouseMove" && _isMouseDown)
        {
            int dx = x - _dragStartX;
            int dy = y - _dragStartY;

            if (!_isDragActive)
            {
                if (Math.Abs(dx) > ROOM_DRAG_THRESHOLD || Math.Abs(dy) > ROOM_DRAG_THRESHOLD)
                {
                    _isDragActive = true;
                }
            }

            if (_isDragActive)
            {
                int offsetX = x - _lastMouseX;
                int offsetY = y - _lastMouseY;

                if (_roomDraggingAlwaysCenters)
                {
                    canvas.ScreenOffsetX += offsetX;
                    canvas.ScreenOffsetY += offsetY;
                }
                else
                {
                    canvas.ScreenOffsetX += offsetX;
                    canvas.ScreenOffsetY += offsetY;
                }

                return true;
            }
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPetType
    private string? GetPetType(string? figure)
    {
        if (figure == null)
        {
            return null;
        }

        // Pet type is derived from the figure string prefix before the first space
        string[] parts = figure.Split(' ', 2);

        if (parts.Length > 0 && int.TryParse(parts[0], out int typeIndex))
        {
            return _roomContentLoader?.GetPetType(typeIndex);
        }

        return null;
    }
    #endregion

    #region Room Data Access
    /// @see com.sulake.habbo.room.RoomEngine::getRoomNumberValue
    public double GetRoomNumberValue(int roomId, string key)
    {
        IRoomObject? roomObj = GetObjectRoom(roomId);
        return roomObj?.Model?.GetNumber(key) ?? 0;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomStringValue
    public string? GetRoomStringValue(int roomId, string key)
    {
        IRoomObject? roomObj = GetObjectRoom(roomId);
        return roomObj?.Model?.GetString(key);
    }

    /// @see com.sulake.habbo.room.RoomEngine::setFurniStackingHeightMap
    public void SetFurniStackingHeightMap(int roomId, FurniStackingHeightMap? heightMap)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);

        if (data != null)
        {
            data.FurniStackingHeightMap = heightMap;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getFurniStackingHeightMap
    public FurniStackingHeightMap? GetFurniStackingHeightMap(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.FurniStackingHeightMap;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getLegacyGeometry
    public LegacyWallGeometry? GetLegacyGeometry(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.LegacyGeometry;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomGeometry
    public RoomGeometry? GetRoomGeometry(int roomId)
    {
        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, 1);
        return canvas?.Geometry as RoomGeometry;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getTileObjectMap
    public TileObjectMap? GetTileObjectMap(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.TileObjectMap;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getSelectedObjectData
    public ISelectedRoomObjectData? GetSelectedObjectData(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.SelectedObject;
    }

    /// @see com.sulake.habbo.room.RoomEngine::setSelectedObjectData
    public void SetSelectedObjectData(int roomId, SelectedRoomObjectData? data)
    {
        RoomInstanceData? instanceData = GetRoomInstanceData(roomId);

        if (instanceData != null)
        {
            instanceData.SelectedObject = data;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::setPlacedObjectData
    public void SetPlacedObjectData(int roomId, SelectedRoomObjectData? data)
    {
        RoomInstanceData? instanceData = GetRoomInstanceData(roomId);

        if (instanceData != null)
        {
            instanceData.PlacedObject = data;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPlacedObjectData
    public ISelectedRoomObjectData? GetPlacedObjectData(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.PlacedObject;
    }

    /// @see com.sulake.habbo.room.RoomEngine::setHanditemControlBlocked
    public void SetHanditemControlBlocked(int roomId, bool blocked)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);

        if (data != null)
        {
            data.HanditemControlBlocked = blocked;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::setIsPlayingGame
    public void SetIsPlayingGame(int roomId, bool isPlaying)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);

        if (data != null)
        {
            data.IsPlayingGame = isPlaying;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getIsPlayingGame
    public bool GetIsPlayingGame(int roomId)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        return data?.IsPlayingGame ?? false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getActiveRoomIsPlayingGame
    public bool GetActiveRoomIsPlayingGame()
    {
        return GetIsPlayingGame(_activeRoomId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::leaveSpectate
    public void LeaveSpectate()
    {
        events.DispatchEvent(new RoomEngineEvent(RoomEngineEvent.ROOM_ENTRANCE_AFTER_SPECTATE, _activeRoomId));
    }

    /// @see com.sulake.habbo.room.RoomEngine::refreshTileObjectMap
    public void RefreshTileObjectMap(int roomId, string category)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        TileObjectMap? tileMap = data?.TileObjectMap;

        if (tileMap != null)
        {
            IRoomInstance? room = GetRoom(roomId);
            List<IRoomObject>? objects = room?.GetObjects(RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE);
            tileMap.Populate(objects?.ToArray() ?? []);
        }

        _eventHandler?.RecalibrateMovements(roomId);
    }
    #endregion

    #region Room Content (IRoomContentListener)
    /// @see com.sulake.habbo.room.RoomEngine::iconLoaded
    public void IconLoaded(int id, string type, bool isWallItem)
    {
        // TODO: Dispatch icon loaded event for catalog/inventory UI
    }
    #endregion

    #region IRoomManagerListener
    /// @see com.sulake.habbo.room.RoomEngine::roomManagerInitialized
    public void RoomManagerInitialized(bool success)
    {
        // Room manager is ready — no additional setup required at this point
    }

    /// @see com.sulake.habbo.room.RoomEngine::contentLoaded
    public void ContentLoaded(string type, bool success)
    {
        // TODO: Implement content loaded callback — generates images for pending requests (Phase 13+)
    }

    /// @see com.sulake.habbo.room.RoomEngine::objectInitialized
    public void ObjectInitialized(string roomId, int objectId, int category)
    {
        int roomIdInt = GetRoomIdFromString(roomId);
        events.DispatchEvent(new RoomEngineObjectEvent(RoomEngineObjectEvent.CONTENT_UPDATED, roomIdInt, objectId, category));
    }

    /// @see com.sulake.habbo.room.RoomEngine::objectsInitialized
    public void ObjectsInitialized(string roomId)
    {
        int roomIdInt = GetRoomIdFromString(roomId);
        CreateRoom(roomIdInt);
    }
    #endregion

    #region Room Creation
    /// @see com.sulake.habbo.room.RoomEngine::createRoom
    private void CreateRoom(int roomId)
    {
        string roomIdStr = GetRoomIdentifier(roomId);
        IRoomInstance? room = _roomManager?.GetRoom(roomIdStr);

        if (room == null)
        {
            return;
        }

        RoomInstanceData instanceData = new(roomId);
        _roomInstanceData![roomIdStr] = instanceData;

        IRoomObjectController? roomObj = room.CreateRoomObject(OBJECT_ID_ROOM, OBJECT_TYPE_ROOM, 0) as IRoomObjectController;

        if (roomObj?.ModelController != null)
        {
            IRoomObjectModelController model = roomObj.ModelController;

            // Store room ID on the room object model
            model.SetNumber("room_id", roomId, true);

            // Initialize legacy wall geometry from room dimensions
            instanceData.LegacyGeometry?.Initialize(
                (int)model.GetNumber("room_x"),
                (int)model.GetNumber("room_y"),
                (int)model.GetNumber("room_z")
            );
        }

        // Create tile cursor (highlighter)
        room.CreateRoomObject(OBJECT_ID_ROOM_HIGHLIGHTER, OBJECT_TYPE_ROOM_HIGHLIGHTER, 200);

        // Create selection arrow
        room.CreateRoomObject(OBJECT_ID_SELECTION_ARROW, OBJECT_TYPE_SELECTION_ARROW, 200);

        events.DispatchEvent(new RoomEngineEvent(RoomEngineEvent.ROOM_INITIALIZED, roomId));
    }
    #endregion

    #region Furniture Creation
    /// @see com.sulake.habbo.room.RoomEngine::createRoomFurniture
    private void CreateRoomFurniture(uint time)
    {
        if (_skipFurnitureCreationForNextFrame)
        {
            _skipFurnitureCreationForNextFrame = false;
            return;
        }

        if (_roomInstanceData == null)
        {
            return;
        }

        long startTime = System.Environment.TickCount64;

        foreach (KeyValuePair<string, RoomInstanceData> kvp in _roomInstanceData)
        {
            RoomInstanceData instanceData = kvp.Value;
            int roomId = instanceData.RoomId;

            while (true)
            {
                FurnitureData? furniData = instanceData.GetFurnitureData();

                if (furniData == null)
                {
                    break;
                }

                AddObjectFurnitureFromData(roomId, furniData);

                if (System.Environment.TickCount64 - startTime >= FURNITURE_CREATION_TIME_BUDGET_MS)
                {
                    _skipFurnitureCreationForNextFrame = true;
                    return;
                }
            }

            while (true)
            {
                FurnitureData? wallData = instanceData.GetWallItemData();

                if (wallData == null)
                {
                    break;
                }

                AddObjectWallItemFromData(roomId, wallData);

                if (System.Environment.TickCount64 - startTime >= FURNITURE_CREATION_TIME_BUDGET_MS)
                {
                    _skipFurnitureCreationForNextFrame = true;
                    return;
                }
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectFurnitureFromData
    private void AddObjectFurnitureFromData(int roomId, FurnitureData data)
    {
        bool needsColorSetup = false;
        string? type = data.Type;

        if (type == null)
        {
            type = GetFurnitureType(data.TypeId);
            needsColorSetup = true;
        }

        int colorIndex = GetFurnitureColorIndex(data.TypeId);
        string adUrl = GetRoomObjectAdUrl(type);

        if (type == null)
        {
            type = "";
        }

        IRoomObjectController? obj = CreateObjectFurniture(roomId, data.Id, type);

        if (obj == null)
        {
            return;
        }

        if (obj.ModelController != null && needsColorSetup)
        {
            IRoomObjectModelController model = obj.ModelController;
            model.SetNumber("furniture_color", colorIndex, true);
            model.SetNumber("furniture_type_id", data.TypeId, true);
            model.SetString("furniture_ad_url", adUrl, true);
            model.SetNumber("furniture_real_room_object", data.RealRoomObject ? 1 : 0, false);
            model.SetNumber("furniture_expiry_time", data.ExpiryTime);
            model.SetNumber("furniture_expirty_timestamp", System.Environment.TickCount64);
            model.SetNumber("furniture_usage_policy", data.UsagePolicy);
            model.SetNumber("furniture_owner_id", data.OwnerId);
            model.SetString("furniture_owner_name", data.OwnerName);
        }

        if (!UpdateObjectFurniture(roomId, data.Id, data.Loc, data.Dir, data.State, data.Data, data.Extra))
        {
            return;
        }

        if (data.SizeZ >= 0)
        {
            UpdateObjectFurnitureHeight(roomId, data.Id, data.SizeZ);
        }

        events.DispatchEvent(new RoomEngineObjectEvent(RoomEngineObjectEvent.ADDED, roomId, data.Id,
            RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE));

        ISelectedRoomObjectData? placed = GetPlacedObjectData(roomId);

        if (placed != null && Math.Abs(placed.Id) == data.Id
                           && placed.Category == RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE)
        {
            SelectRoomObject(roomId, data.Id, RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE);
        }

        if (obj.IsInitialized && data.Synchronized)
        {
            AddObjectToTileMap(roomId, obj);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectWallItemFromData
    private void AddObjectWallItemFromData(int roomId, FurnitureData data)
    {
        string legacyString = "";

        if (data.Data != null)
        {
            legacyString = data.Data.GetLegacyString();
        }

        string? type = GetWallItemType(data.TypeId, legacyString);
        int colorIndex = GetWallItemColorIndex(data.TypeId);
        string adUrl = GetRoomObjectAdUrl(type);

        if (type == null)
        {
            type = "";
        }

        IRoomObjectController? obj = CreateObjectWallItem(roomId, data.Id, type);

        if (obj == null)
        {
            return;
        }

        if (obj.ModelController != null)
        {
            IRoomObjectModelController model = obj.ModelController;
            model.SetNumber("furniture_color", colorIndex, false);
            model.SetNumber("furniture_type_id", data.TypeId, true);
            model.SetString("furniture_ad_url", adUrl, true);
            model.SetNumber("furniture_real_room_object", data.RealRoomObject ? 1 : 0, false);
            model.SetNumber("object_accurate_z_value", 1, true);
            model.SetNumber("furniture_usage_policy", data.UsagePolicy);
            model.SetNumber("furniture_expiry_time", data.ExpiryTime);
            model.SetNumber("furniture_expirty_timestamp", System.Environment.TickCount64);
            model.SetNumber("furniture_owner_id", data.OwnerId);
            model.SetString("furniture_owner_name", data.OwnerName);
        }

        legacyString = "";

        if (data.Data != null)
        {
            legacyString = data.Data.GetLegacyString();
        }

        if (!UpdateObjectWallItem(roomId, data.Id, data.Loc, data.Dir, data.State, legacyString))
        {
            return;
        }

        events.DispatchEvent(new RoomEngineObjectEvent(RoomEngineObjectEvent.ADDED, roomId, data.Id,
            RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL));

        ISelectedRoomObjectData? placed = GetPlacedObjectData(roomId);

        if (placed != null && placed.Id == data.Id
                           && placed.Category == RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL)
        {
            SelectRoomObject(roomId, data.Id, RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::createObjectFromData
    private IRoomObject? CreateObjectFromData(int roomId, int objectId, int category)
    {
        RoomInstanceData? instanceData = GetRoomInstanceData(roomId);

        if (instanceData == null)
        {
            return null;
        }

        if (category == 10)
        {
            FurnitureData? furniData = instanceData.GetFurnitureDataWithId(objectId);

            if (furniData != null)
            {
                AddObjectFurnitureFromData(roomId, furniData);
                return GetRoom(roomId)?.GetObject(objectId, category);
            }
        }
        else if (category == 20)
        {
            FurnitureData? wallData = instanceData.GetWallItemDataWithId(objectId);

            if (wallData != null)
            {
                AddObjectWallItemFromData(roomId, wallData);
                return GetRoom(roomId)?.GetObject(objectId, category);
            }
        }

        return null;
    }
    #endregion

    #region Camera Management
    /// @see com.sulake.habbo.room.RoomEngine::updateRoomCameras
    private void UpdateRoomCameras(uint time)
    {
        if (_roomInstanceData == null)
        {
            return;
        }

        foreach (KeyValuePair<string, RoomInstanceData> kvp in _roomInstanceData)
        {
            RoomInstanceData instanceData = kvp.Value;
            RoomCamera? camera = instanceData.RoomCamera;

            if (camera == null || !camera.IsMoving)
            {
                continue;
            }

            camera.Update(time, 1.0);

            // TODO: Apply camera offset to room canvas when rendering pipeline is wired
        }
    }
    #endregion

    #region Canvas Management (stubs)
    /// @see com.sulake.habbo.room.RoomEngine::createRoomCanvas
    public Node2D? CreateRoomCanvas(int roomId, int canvasId, int width, int height, int scale)
    {
        IRoomInstance? room = GetRoom(roomId);

        if (room == null)
        {
            return null;
        }

        IRoomRenderer? renderer = room.GetRenderer() as IRoomRenderer;

        if (renderer == null && _roomRendererFactory != null)
        {
            renderer = _roomRendererFactory.CreateRenderer() as IRoomRenderer;
        }

        if (renderer == null)
        {
            return null;
        }

        renderer.RoomObjectVariableAccurateZ = "object_accurate_z_value";
        room.SetRenderer(renderer);

        IRoomRenderingCanvas? canvas = renderer.CreateCanvas(canvasId, width, height, scale);

        if (canvas == null)
        {
            return null;
        }

        canvas.MouseListener = _eventHandler;

        if (canvas.Geometry != null)
        {
            canvas.Geometry.ZScale = room.GetNumber("room_z_scale");
        }

        if (canvas.Geometry != null)
        {
            double doorX = room.GetNumber("room_door_x");
            double doorY = room.GetNumber("room_door_y");
            double doorZ = room.GetNumber("room_door_z");
            double doorDir = room.GetNumber("room_door_dir");

            IVector3d doorPos = new Vector3d(doorX, doorY, doorZ);
            IVector3d displacement = new Vector3d(0, 0, 0);

            if (doorDir == 90)
            {
                displacement = new Vector3d(-2000, 0, 0);
            }
            else if (doorDir == 180)
            {
                displacement = new Vector3d(0, -2000, 0);
            }

            canvas.Geometry.SetDisplacement(doorPos, displacement);
        }

        return canvas.DisplayObject;
    }

    /// @see com.sulake.habbo.room.RoomEngine::setRoomCanvasScale
    public void SetRoomCanvasScale(int roomId, int canvasId, double scale, Vector2? point = null,
        Vector2? offset = null, bool skipUpdate = false, bool center = false, bool noAnimation = false)
    {
        if (!GetBoolean("zoom.enabled"))
        {
            return;
        }

        if (!center)
        {
            scale = skipUpdate ? -1 : (scale < 1 ? 0.5 : Math.Floor(scale));
        }

        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);

        if (canvas != null)
        {
            canvas.SetScale(scale, point, offset, noAnimation);
            events.DispatchEvent(new RoomEngineEvent(RoomEngineEvent.ROOM_ZOOMED, roomId));
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::modifyRoomCanvas
    public bool ModifyRoomCanvas(int roomId, int canvasId, int width, int height)
    {
        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);

        if (canvas == null)
        {
            return false;
        }

        canvas.Initialize(width, height);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::setRoomCanvasMask
    public void SetRoomCanvasMask(int roomId, int canvasId, bool flag)
    {
        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);

        if (canvas != null)
        {
            canvas.UseMask = flag;
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomCanvasGeometry
    public IRoomGeometry? GetRoomCanvasGeometry(int roomId, int canvasId = -1)
    {
        if (canvasId == -1)
        {
            canvasId = _activeCanvasId;
        }

        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);
        return canvas?.Geometry;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomCanvasScreenOffset
    public Vector2? GetRoomCanvasScreenOffset(int roomId, int canvasId = -1)
    {
        if (canvasId == -1)
        {
            canvasId = _activeCanvasId;
        }

        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);

        if (canvas != null)
        {
            return new Vector2(canvas.ScreenOffsetX, canvas.ScreenOffsetY);
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::setRoomCanvasScreenOffset
    public bool SetRoomCanvasScreenOffset(int roomId, int canvasId, Vector2 offset)
    {
        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);

        if (canvas == null)
        {
            return false;
        }

        canvas.ScreenOffsetX = (int)offset.X;
        canvas.ScreenOffsetY = (int)offset.Y;
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomCanvasScale
    public double GetRoomCanvasScale(int roomId = -1000, int canvasId = -1)
    {
        if (roomId == -1000)
        {
            roomId = _activeRoomId;
        }

        if (canvasId == -1)
        {
            canvasId = _activeCanvasId;
        }

        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);
        return canvas?.Scale ?? 1.0;
    }

    /// @see com.sulake.habbo.room.RoomEngine::handleRoomCanvasMouseEvent
    public void HandleRoomCanvasMouseEvent(int roomId, int canvasId, int x, int y, string type,
        bool altKey, bool ctrlKey, bool shiftKey, bool buttonDown)
    {
        if (MouseEventsDisabledAboveY > 0 && y < MouseEventsDisabledAboveY)
        {
            return;
        }

        if (MouseEventsDisabledLeftToX > 0 && x < MouseEventsDisabledLeftToX)
        {
            return;
        }

        IRoomRenderingCanvas? canvas = GetRoomCanvas(_activeRoomId, canvasId);

        if (canvas == null)
        {
            return;
        }

        // TODO: Zoom on ctrl+alt+click when _sessionDataManager.IsPerkAllowed("MOUSE_ZOOM") is ported

        if (!HandleRoomDragging(canvas, x, y, type, altKey, ctrlKey, shiftKey))
        {
            if (!canvas.HandleMouseEvent(x, y, type, altKey, ctrlKey, shiftKey, buttonDown))
            {
                string eventType = "";

                if (type == "click")
                {
                    events.DispatchEvent(new RoomEngineObjectEvent(
                        RoomEngineObjectEvent.DESELECTED, _activeRoomId, -1, -2));
                    eventType = Vortex.Room.Events.RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK;
                }
                else if (type == "mouseMove")
                {
                    eventType = Vortex.Room.Events.RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_MOVE;
                }
                else if (type == "mouseDown")
                {
                    eventType = Vortex.Room.Events.RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_DOWN;
                }

                if (eventType.Length > 0 && _eventHandler != null)
                {
                    _eventHandler.HandleRoomObjectEvent(
                        new Vortex.Room.Events.RoomObjectMouseEvent(
                            eventType, GetRoomObject(_activeRoomId, OBJECT_ID_ROOM, 0), "", altKey),
                        _activeRoomId);
                }
            }
        }

        _activeCanvasId = canvasId;
        _lastMouseX = x;
        _lastMouseY = y;
    }
    #endregion

    #region Object Manipulation (stubs)
    /// @see com.sulake.habbo.room.RoomEngine::modifyRoomObject
    public bool ModifyRoomObject(int objectId, int category, string operation)
    {
        if (_eventHandler != null)
        {
            return _eventHandler.ModifyRoomObject(_activeRoomId, objectId, category, operation);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::modifyRoomObjectDataWithMap
    public bool ModifyRoomObjectDataWithMap(int objectId, int category, string operation, Dictionary<string, string> data)
    {
        if (_eventHandler != null && category == RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE)
        {
            return _eventHandler.ModifyRoomObjectData(_activeRoomId, objectId, category, operation, data);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::modifyRoomObjectData
    public bool ModifyRoomObjectData(int objectId, int category, string operation, string data)
    {
        if (_eventHandler != null && category == RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL)
        {
            return _eventHandler.ModifyWallItemData(_activeRoomId, objectId, operation, data);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::deleteRoomObject
    public bool DeleteRoomObject(int objectId, int category)
    {
        if (_eventHandler != null && category == RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL)
        {
            return _eventHandler.DeleteWallItem(_activeRoomId, objectId);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::initializeRoomObjectInsert
    public bool InitializeRoomObjectInsert(string type, int typeId, int category, int stuffDataKey,
        string? extra = null, IStuffData? stuffData = null, int state = -1, int frameCount = -1,
        string? instanceData = null, bool isReplace = false)
    {
        IRoomInstance? room = GetRoom(_activeRoomId);

        if (room == null || room.GetNumber("room_is_public") != 0)
        {
            return false;
        }

        if (_eventHandler != null)
        {
            return _eventHandler.InitializeRoomObjectInsert(
                type, _activeRoomId, typeId, category, stuffDataKey,
                extra, stuffData, state, frameCount, instanceData);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::cancelRoomObjectInsert
    public void CancelRoomObjectInsert()
    {
        _eventHandler?.CancelRoomObjectInsert(_activeRoomId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::selectAvatar
    public void SelectAvatar(int roomId, int objectId)
    {
        _eventHandler?.SetSelectedAvatar(roomId, objectId, true);
    }

    /// @see com.sulake.habbo.room.RoomEngine::selectRoomObject
    public void SelectRoomObject(int roomId, int objectId, int category)
    {
        _eventHandler?.SetSelectedObject(roomId, objectId, category);
    }

    /// @see com.sulake.habbo.room.RoomEngine::useRoomObjectInActiveRoom
    public bool UseRoomObjectInActiveRoom(int objectId, int category)
    {
        IRoomObject? obj = GetRoomObject(_activeRoomId, objectId, category);

        if (obj != null)
        {
            Vortex.Room.Object.Logic.IRoomObjectEventHandler? handler =
                obj.MouseHandler as Vortex.Room.Object.Logic.IRoomObjectEventHandler;

            if (handler != null)
            {
                handler.UseObject();
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Object CRUD (IRoomObjectCreator / IRoomCreator)
    /// @see com.sulake.habbo.room.RoomEngine::addObjectFurniture
    public bool AddObjectFurniture(int roomId, int objectId, int typeId, IVector3d location,
        IVector3d direction, int state, IStuffData stuffData, double extra = double.NaN,
        int expiryTime = -1, int usagePolicy = 0, int ownerId = 0, string ownerName = "",
        bool synchronized = true, bool realRoomObject = true, double sizeZ = -1)
    {
        RoomInstanceData? instanceData = GetRoomInstanceData(roomId);

        if (instanceData == null)
        {
            return false;
        }

        string? type = _roomContentLoader?.GetActiveObjectType(typeId);
        FurnitureData data = new(objectId, typeId, type, location, direction, state, stuffData,
            extra, expiryTime, usagePolicy, ownerId, ownerName, synchronized, realRoomObject, sizeZ);

        instanceData.AddFurnitureData(data);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectWallItem
    public bool AddObjectWallItem(int roomId, int objectId, int typeId, IVector3d location,
        IVector3d direction, int state, string data, int usagePolicy = 0, int ownerId = 0,
        string ownerName = "", int expiryTime = -1, bool realRoomObject = true)
    {
        RoomInstanceData? instanceData = GetRoomInstanceData(roomId);

        if (instanceData == null)
        {
            return false;
        }

        string? type = _roomContentLoader?.GetWallItemType(typeId);
        FurnitureData furniData = new(objectId, typeId, type, location, direction, state, null,
            double.NaN, expiryTime, usagePolicy, ownerId, ownerName, true, realRoomObject, -1, data);

        instanceData.AddWallItemData(furniData);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectUser
    public bool AddObjectUser(int roomId, int objectId, IVector3d location, IVector3d direction,
        double headDirection, int userType, string? figure = null)
    {
        if (GetObjectUser(roomId, objectId) != null)
        {
            return false;
        }

        string? type = RoomObjectUserTypes.GetName(userType);

        if (type == "pet")
        {
            type = GetPetType(figure);
        }

        IRoomObjectController? obj = CreateObjectUser(roomId, objectId, type ?? "user");

        if (obj == null)
        {
            return false;
        }

        if (obj.EventHandler != null)
        {
            RoomObjectAvatarUpdateMessage updateMsg = new(
                FixedUserLocation(roomId, location), null, direction,
                (int)headDirection, false, 0);
            obj.EventHandler.ProcessUpdateMessage(updateMsg);

            if (figure != null)
            {
                RoomObjectAvatarFigureUpdateMessage figureMsg = new(figure);
                obj.EventHandler.ProcessUpdateMessage(figureMsg);
            }
        }

        events.DispatchEvent(new RoomEngineObjectEvent(RoomEngineObjectEvent.ADDED, roomId, objectId,
            RoomObjectCategoryEnum.OBJECT_CATEGORY_USER));

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectSnowWar
    public bool AddObjectSnowWar(int roomId, int objectId, IVector3d location, int frame)
    {
        string? type = null;

        if (frame == 201)
        {
            type = "game_snowball";
        }
        else if (frame == 202)
        {
            type = "game_snowsplash";
        }

        IRoomObjectController? obj = CreateObjectSnowWar(roomId, objectId, type ?? "", frame);

        if (obj == null)
        {
            return false;
        }

        if (obj.EventHandler != null)
        {
            obj.EventHandler.ProcessUpdateMessage(new RoomObjectUpdateMessage(location, null));
        }

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUser
    public bool UpdateObjectUser(int roomId, int objectId, IVector3d location, IVector3d direction,
        bool canStandUp = false, double baseY = 0, IVector3d? targetLocation = null,
        double headDirection = double.NaN, double countdownTime = double.NaN,
        bool isSlide = false)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null || obj.Model == null)
        {
            return false;
        }

        if (double.IsNaN(headDirection))
        {
            headDirection = obj.Model.GetNumber("head_direction");
        }

        RoomObjectAvatarUpdateMessage msg = new(
            FixedUserLocation(roomId, location),
            FixedUserLocation(roomId, targetLocation),
            direction, (int)headDirection, canStandUp, baseY,
            countdownTime, isSlide);
        obj.EventHandler.ProcessUpdateMessage(msg);

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserFigure
    public bool UpdateObjectUserFigure(int roomId, int objectId, string figure, string? gender = null,
        string? club = null, bool isRiding = false)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarFigureUpdateMessage(figure, gender, club, isRiding));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserPosture
    public bool UpdateObjectUserPosture(int roomId, int objectId, string posture, string parameter = "")
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarPostureUpdateMessage(posture, parameter));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserGesture
    public bool UpdateObjectUserGesture(int roomId, int objectId, int gesture)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarGestureUpdateMessage(gesture));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserEffect
    public bool UpdateObjectUserEffect(int roomId, int objectId, int effectId, int delay = 0)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarEffectUpdateMessage(effectId, delay));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectSnowWar
    public bool UpdateObjectSnowWar(int roomId, int objectId, IVector3d location, int frame)
    {
        IRoomObjectController? obj = GetRoom(roomId)?.GetObject(objectId, frame) as IRoomObjectController;

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectUpdateMessage(location, null));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::disposeObjectSnowWar
    public void DisposeObjectSnowWar(int roomId, int objectId, int delay)
    {
        DisposeObject(roomId, objectId, delay);
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserAction
    public bool UpdateObjectUserAction(int roomId, int objectId, string action, int value,
        string? parameter = null)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        RoomObjectUpdateStateMessage? msg = action switch
        {
            "figure_talk" => new RoomObjectAvatarChatUpdateMessage(value),
            "figure_sleep" => new RoomObjectAvatarSleepUpdateMessage(value != 0),
            "figure_is_typing" => new RoomObjectAvatarTypingUpdateMessage(value != 0),
            "figure_is_muted" => new RoomObjectAvatarMutedUpdateMessage(value != 0),
            "figure_carry_object" => new RoomObjectAvatarCarryObjectUpdateMessage(value, parameter ?? ""),
            "figure_use_object" => new RoomObjectAvatarUseObjectUpdateMessage(value),
            "figure_dance" => new RoomObjectAvatarDanceUpdateMessage(value),
            "figure_gained_experience" => new RoomObjectAvatarExperienceUpdateMessage(value),
            "figure_number_value" => new RoomObjectAvatarPlayerValueUpdateMessage(value),
            "figure_sign" => new RoomObjectAvatarSignUpdateMessage(value),
            "figure_expression" => new RoomObjectAvatarExpressionUpdateMessage(value),
            "figure_is_playing_game" => new RoomObjectAvatarPlayingGameMessage(value != 0),
            "figure_guide_status" => new RoomObjectAvatarGuideStatusUpdateMessage(value),
            _ => null,
        };

        if (msg != null)
        {
            obj.EventHandler.ProcessUpdateMessage(msg);
        }

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::disposeObjectUser
    public void DisposeObjectUser(int roomId, int objectId)
    {
        IRoomInstance? room = GetRoom(roomId);
        room?.DisposeObject(objectId, 100);
    }

    /// @see com.sulake.habbo.room.RoomEngine::changeObjectState
    public void ChangeObjectState(int roomId, int objectId, int category)
    {
        string roomIdStr = GetRoomIdentifier(roomId);
        IRoomObjectController? obj = GetRoom(roomId)?.GetObject(objectId, category) as IRoomObjectController;

        if (obj?.ModelController != null)
        {
            double stateIndex = obj.ModelController.GetNumber("furniture_automatic_state_index");

            if (double.IsNaN(stateIndex))
            {
                stateIndex = 1;
            }
            else
            {
                stateIndex += 1;
            }

            obj.ModelController.SetNumber("furniture_automatic_state_index", stateIndex);

            int dataFormat = (int)obj.Model.GetNumber("furniture_data_format");
            IStuffData? stuffData = StuffDataFactory.GetStuffDataWrapperForType(dataFormat);
            stuffData?.InitializeFromRoomObjectModel(obj.Model);

            RoomObjectDataUpdateMessage msg = new((int)stateIndex, stuffData);

            obj.EventHandler?.ProcessUpdateMessage(msg);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::changeObjectModelData
    public bool ChangeObjectModelData(int roomId, int objectId, int category, string key, int value)
    {
        IRoomObjectController? obj = GetRoom(roomId)?.GetObject(objectId, category) as IRoomObjectController;

        if (obj == null)
        {
            return false;
        }

        if (obj.EventHandler != null)
        {
            obj.EventHandler.ProcessUpdateMessage(new RoomObjectModelDataUpdateMessage(key, value));
        }

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::disposeObjectFurniture
    public void DisposeObjectFurniture(int roomId, int objectId, int delay = -1, bool expired = false)
    {
        RoomInstanceData? data = GetRoomInstanceData(roomId);
        data?.GetFurnitureDataWithId(objectId);

        DisposeObject(roomId, objectId, RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE);

        if (expired)
        {
            RefreshTileObjectMap(roomId, "RoomEngine.DisposeObjectFurniture()");
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectWallItemLocation
    public bool UpdateObjectWallItemLocation(int roomId, int objectId, IVector3d location,
        IVector3d? direction = null, double extra = double.NaN)
    {
        IRoomObjectController? obj = GetObjectWallItem(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        RoomObjectMoveUpdateMessage msg = new(location, direction, null, extra, direction != null);
        obj.EventHandler.ProcessUpdateMessage(msg);
        UpdateObjectRoomWindow(roomId, objectId);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::disposeObjectWallItem
    public void DisposeObjectWallItem(int roomId, int objectId, int delay = -1)
    {
        IRoomInstance? room = GetRoom(roomId);
        room?.DisposeObject(objectId, 20);
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectRoom
    public bool UpdateObjectRoom(int roomId, string? floorType = null, string? wallType = null,
        string? landscapeType = null, bool animate = false)
    {
        IRoomObjectController? roomObj = GetObjectRoom(roomId);
        IRoomInstance? room = GetRoom(roomId);

        if (roomObj == null)
        {
            // Room not yet created — store pending data
            RoomInstanceData? pendingData = GetRoomInstanceData(roomId);

            if (pendingData != null)
            {
                if (floorType != null)
                {
                    pendingData.PendingFloorType = floorType;
                }

                if (wallType != null)
                {
                    pendingData.PendingWallType = wallType;
                }

                if (landscapeType != null)
                {
                    pendingData.PendingLandscapeType = landscapeType;
                }
            }

            return true;
        }

        if (roomObj.EventHandler == null)
        {
            return false;
        }

        if (floorType != null)
        {
            if (room != null && !animate)
            {
                room.SetString("room_floor_type", floorType);
            }

            roomObj.EventHandler.ProcessUpdateMessage(
                new RoomObjectRoomUpdateMessage(RoomObjectRoomUpdateMessage.ROOM_FLOOR_UPDATE, floorType));
        }

        if (wallType != null)
        {
            if (room != null && !animate)
            {
                room.SetString("room_wall_type", wallType);
            }

            roomObj.EventHandler.ProcessUpdateMessage(
                new RoomObjectRoomUpdateMessage(RoomObjectRoomUpdateMessage.ROOM_WALL_UPDATE, wallType));
        }

        if (landscapeType != null)
        {
            if (room != null && !animate)
            {
                room.SetString("room_landscape_type", landscapeType);
            }

            roomObj.EventHandler.ProcessUpdateMessage(
                new RoomObjectRoomUpdateMessage(RoomObjectRoomUpdateMessage.ROOM_LANDSCAPE_UPDATE, landscapeType));
        }

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectRoomColor
    public bool UpdateObjectRoomColor(int roomId, uint color, int brightness, bool bgOnly)
    {
        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler == null)
        {
            return false;
        }

        roomObj.EventHandler.ProcessUpdateMessage(
            new RoomObjectRoomColorUpdateMessage(RoomObjectRoomColorUpdateMessage.BACKGROUND_COLOR, color, brightness, bgOnly));
        events.DispatchEvent(new RoomEngineRoomColorEvent(roomId, color, (uint)brightness, bgOnly));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectRoomBackgroundColor
    public bool UpdateObjectRoomBackgroundColor(int roomId, bool enable, int hue, int saturation, int lightness)
    {
        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler == null)
        {
            return false;
        }

        events.DispatchEvent(new RoomEngineHSLColorEnableEvent(
            RoomEngineHSLColorEnableEvent.ROOM_BACKGROUND_COLOR, roomId, enable, hue, saturation, lightness));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectRoomVisibilities
    public bool UpdateObjectRoomVisibilities(int roomId, bool wallsVisible, bool floorVisible = true)
    {
        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler == null)
        {
            return false;
        }

        roomObj.EventHandler.ProcessUpdateMessage(
            new RoomObjectRoomPlaneVisibilityUpdateMessage(RoomObjectRoomPlaneVisibilityUpdateMessage.WALL_VISIBILITY, wallsVisible));
        roomObj.EventHandler.ProcessUpdateMessage(
            new RoomObjectRoomPlaneVisibilityUpdateMessage(RoomObjectRoomPlaneVisibilityUpdateMessage.FLOOR_VISIBILITY, floorVisible));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectFurnitureByName
    public bool AddObjectFurnitureByName(int roomId, int objectId, string type, IVector3d location,
        IVector3d direction, int state, IStuffData stuffData, double extra = double.NaN)
    {
        RoomInstanceData? instanceData = GetRoomInstanceData(roomId);

        if (instanceData != null)
        {
            FurnitureData furniData = new(objectId, 0, type, location, direction, state, stuffData, extra, 0);
            instanceData.AddFurnitureData(furniData);
        }

        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectFurniture
    public bool UpdateObjectFurniture(int roomId, int objectId, IVector3d location, IVector3d direction,
        int state, IStuffData? stuffData, double extra = double.NaN)
    {
        IRoomObjectController? obj = GetObjectFurniture(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectUpdateMessage(location, direction));
        obj.EventHandler.ProcessUpdateMessage(new RoomObjectDataUpdateMessage(state, stuffData, extra));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectFurnitureHeight
    public bool UpdateObjectFurnitureHeight(int roomId, int objectId, double height)
    {
        IRoomObjectController? obj = GetObjectFurniture(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectHeightUpdateMessage(null, null, height));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectFurnitureLocation
    public bool UpdateObjectFurnitureLocation(int roomId, int objectId, IVector3d location,
        IVector3d direction, IVector3d? targetLocation = null, double extra = double.NaN)
    {
        IRoomObjectController? obj = GetObjectFurniture(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        RoomObjectMoveUpdateMessage msg = new(location, targetLocation, direction, extra, targetLocation != null);
        obj.EventHandler.ProcessUpdateMessage(msg);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectFurnitureExpiryTime
    public bool UpdateObjectFurnitureExpiryTime(int roomId, int objectId, int expiryTime)
    {
        IRoomObjectController? obj = GetObjectFurniture(roomId, objectId);

        if (obj?.ModelController == null)
        {
            return false;
        }

        obj.ModelController.SetNumber("furniture_expiry_time", expiryTime);
        obj.ModelController.SetNumber("furniture_expirty_timestamp", System.Environment.TickCount64);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectWallItem
    public bool UpdateObjectWallItem(int roomId, int objectId, IVector3d location, IVector3d direction,
        int state, string data)
    {
        IRoomObjectController? obj = GetObjectWallItem(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        LegacyStuffData stuffData = new();
        stuffData.SetString(data);

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectUpdateMessage(location, direction));
        obj.EventHandler.ProcessUpdateMessage(new RoomObjectDataUpdateMessage(state, stuffData));
        UpdateObjectRoomWindow(roomId, objectId);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectWallItemState
    public bool UpdateObjectWallItemState(int roomId, int objectId, int state, string data)
    {
        IRoomObjectController? obj = GetObjectWallItem(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        LegacyStuffData stuffData = new();
        stuffData.SetString(data);

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectDataUpdateMessage(state, stuffData));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectWallItemData
    public bool UpdateObjectWallItemData(int roomId, int objectId, string data)
    {
        IRoomObjectController? obj = GetObjectWallItem(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectItemDataUpdateMessage(data));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectWallItemExpiryTime
    public bool UpdateObjectWallItemExpiryTime(int roomId, int objectId, int expiryTime)
    {
        IRoomObjectController? obj = GetObjectWallItem(roomId, objectId);

        if (obj?.ModelController == null)
        {
            return false;
        }

        obj.ModelController.SetNumber("furniture_expiry_time", expiryTime);
        obj.ModelController.SetNumber("furniture_expirty_timestamp", System.Environment.TickCount64);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserDir
    public bool UpdateObjectUserDir(int roomId, int objectId, IVector3d direction, double headDirection)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null || obj.Model == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(
            new RoomObjectAvatarDirectionUpdateMessage(null, direction, (int)headDirection));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserFlatControl
    public bool UpdateObjectUserFlatControl(int roomId, int objectId, string level)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarFlatControlUpdateMessage(level));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectUserOwnUserAvatar
    public bool UpdateObjectUserOwnUserAvatar(int roomId, int objectId)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarOwnMessage());
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectPetGesture
    public bool UpdateObjectPetGesture(int roomId, int objectId, string gesture)
    {
        IRoomObjectController? obj = GetObjectUser(roomId, objectId);

        if (obj?.EventHandler == null)
        {
            return false;
        }

        obj.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarPetGestureUpdateMessage(gesture));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateObjectRoomPlaneThicknesses
    public bool UpdateObjectRoomPlaneThicknesses(int roomId, double wallThickness, double floorThickness)
    {
        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler == null)
        {
            return false;
        }

        roomObj.EventHandler.ProcessUpdateMessage(
            new RoomObjectRoomPlanePropertyUpdateMessage(RoomObjectRoomPlanePropertyUpdateMessage.WALL_THICKNESS, wallThickness));
        roomObj.EventHandler.ProcessUpdateMessage(
            new RoomObjectRoomPlanePropertyUpdateMessage(RoomObjectRoomPlanePropertyUpdateMessage.FLOOR_THICKNESS, floorThickness));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::updateAreaHide
    public bool UpdateAreaHide(int roomId, int objectId, bool isOn, int rootX, int rootY,
        int width, int height, bool invert)
    {
        events.DispatchEvent(new RoomEngineAreaHideStateWidgetEvent(roomId, objectId,
            RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE, isOn));

        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler == null)
        {
            return false;
        }

        RoomObjectRoomFloorHoleUpdateMessage msg;

        if (isOn)
        {
            msg = new RoomObjectRoomFloorHoleUpdateMessage(
                RoomObjectRoomFloorHoleUpdateMessage.ADD_HOLE, objectId, rootX, rootY, width, height, invert);
        }
        else
        {
            msg = new RoomObjectRoomFloorHoleUpdateMessage(
                RoomObjectRoomFloorHoleUpdateMessage.REMOVE_HOLE, objectId);
        }

        roomObj.EventHandler.ProcessUpdateMessage(msg);
        return true;
    }

    /// @see com.sulake.habbo.room.RoomEngine::setRoomObjectAlias
    public void SetRoomObjectAlias(string alias, string target)
    {
        _roomContentLoader?.SetRoomObjectAlias(alias, target);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPetTypeId
    public int GetPetTypeId(string type)
    {
        return _roomContentLoader?.GetPetTypeId(type) ?? 0;
    }
    #endregion

    #region Image Generation (stubs)
    /// @see com.sulake.habbo.room.RoomEngine::getFurnitureIconUrl
    public string? GetFurnitureIconUrl(int typeId)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getFurnitureIcon
    public ImageResult? GetFurnitureIcon(int typeId, IGetImageListener? listener, string? extra = null,
        IStuffData? stuffData = null, bool isIcon = false)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getWallItemIconUrl
    public string? GetWallItemIconUrl(int typeId, string? extra = null)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getWallItemIcon
    public ImageResult? GetWallItemIcon(int typeId, IGetImageListener? listener, string? extra = null)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getFurnitureImage
    public ImageResult? GetFurnitureImage(int typeId, IVector3d direction, int scale,
        IGetImageListener? listener, uint bgColor = 0, string? extra = null,
        int state = -1, int frameCount = -1, IStuffData? stuffData = null, bool isIcon = false)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getGenericRoomObjectImage
    public ImageResult? GetGenericRoomObjectImage(string type, string value, IVector3d direction,
        int scale, IGetImageListener? listener, uint bgColor = 0, string? extra = null,
        IStuffData? stuffData = null, int state = -1, int frameCount = -1,
        string? posture = null, int entityId = -1)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getWallItemImage
    public ImageResult? GetWallItemImage(int typeId, IVector3d direction, int scale,
        IGetImageListener? listener, uint bgColor = 0, string? extra = null,
        int state = -1, int frameCount = -1)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPetImage
    public ImageResult? GetPetImage(int typeId, int paletteId, int customPartCount,
        IVector3d direction, int scale, IGetImageListener? listener,
        bool headOnly = true, uint bgColor = 0, string[]? customParts = null,
        string? posture = null)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomImage
    public ImageResult? GetRoomImage(string floorType, string wallType, string landscapeType,
        int scale, IGetImageListener? listener, string? xml = null)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectImage
    public ImageResult? GetRoomObjectImage(int roomId, int objectId, int category,
        IVector3d direction, int scale, IGetImageListener? listener, uint bgColor = 0)
    {
        // TODO: Implement (Phase 13+)
        return null;
    }
    #endregion

    #region Pet Colors
    /// @see com.sulake.habbo.room.RoomEngine::getPetColor
    public PetColorResult? GetPetColor(int typeId, int paletteId)
    {
        return _roomContentLoader?.GetPetColor(typeId, paletteId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPetColorsByTag
    public string[]? GetPetColorsByTag(int typeId, string tag)
    {
        List<PetColorResult>? results = _roomContentLoader?.GetPetColorsByTag(typeId, tag);

        if (results == null || results.Count == 0)
        {
            return null;
        }

        string[] ids = new string[results.Count];

        for (int i = 0; i < results.Count; i++)
        {
            ids[i] = results[i].Id;
        }

        return ids;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPetLayerIdForTag
    public int GetPetLayerIdForTag(int typeId, string tag)
    {
        return _roomContentLoader?.GetPetLayerIdForTag(typeId, tag) ?? -1;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getPetDefaultPalette
    public PetColorResult? GetPetDefaultPalette(int typeId, string tag)
    {
        return _roomContentLoader?.GetPetDefaultPalette(typeId, tag);
    }
    #endregion

    #region Type Resolution
    /// @see com.sulake.habbo.room.RoomEngine::getFurnitureType
    public string? GetFurnitureType(int typeId)
    {
        return _roomContentLoader?.GetActiveObjectType(typeId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getFurnitureTypeId
    public int GetFurnitureTypeId(string type)
    {
        return _roomContentLoader?.GetActiveObjectTypeId(type) ?? 0;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getWallItemType
    public string? GetWallItemType(int typeId, string? extra = null)
    {
        return _roomContentLoader?.GetWallItemType(typeId, extra);
    }
    #endregion

    #region Remaining Stubs
    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectBoundingRectangle
    public Rect2? GetRoomObjectBoundingRectangle(int roomId, int objectId, int category, int canvasId)
    {
        IRoomGeometry? geometry = GetRoomCanvasGeometry(roomId, canvasId);

        if (geometry == null)
        {
            return null;
        }

        IRoomObject? obj = GetRoomObject(roomId, objectId, category);

        if (obj?.Visualization == null)
        {
            return null;
        }

        Rect2I intBounds = obj.Visualization.BoundingRectangle;
        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);
        double scale = canvas?.Scale ?? 1.0;

        Vector2? screenPoint = geometry.GetScreenPoint(obj.Location);

        if (screenPoint == null)
        {
            return null;
        }

        float left = (float)(intBounds.Position.X * scale);
        float top = (float)(intBounds.Position.Y * scale);
        float width = (float)(intBounds.Size.X * scale);
        float height = (float)(intBounds.Size.Y * scale);
        float screenX = (float)(screenPoint.Value.X * scale);
        float screenY = (float)(screenPoint.Value.Y * scale);

        left += screenX;
        top += screenY;

        if (canvas != null)
        {
            left += (canvas.Width / 2) + canvas.ScreenOffsetX;
            top += (canvas.Height / 2) + canvas.ScreenOffsetY;
            return new Rect2(left, top, width, height);
        }

        return null;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getRoomObjectScreenLocation
    public Vector2? GetRoomObjectScreenLocation(int roomId, int objectId, int category, int canvasId = -1)
    {
        if (canvasId == -1)
        {
            canvasId = _activeCanvasId;
        }

        IRoomGeometry? geometry = GetRoomCanvasGeometry(roomId, canvasId);

        if (geometry == null)
        {
            return null;
        }

        IRoomObject? obj = GetRoomObject(roomId, objectId, category);

        if (obj == null)
        {
            return null;
        }

        Vector2? point = geometry.GetScreenPoint(obj.Location);

        if (point == null)
        {
            return null;
        }

        IRoomRenderingCanvas? canvas = GetRoomCanvas(roomId, canvasId);

        if (canvas != null)
        {
            float x = (float)(point.Value.X * canvas.Scale);
            float y = (float)(point.Value.Y * canvas.Scale);
            x += (canvas.Width / 2) + canvas.ScreenOffsetX;
            y += (canvas.Height / 2) + canvas.ScreenOffsetY;
            return new Vector2(x, y);
        }

        return point;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getActiveRoomBoundingRectangle
    public Rect2? GetActiveRoomBoundingRectangle(int canvasId)
    {
        return GetRoomObjectBoundingRectangle(_activeRoomId, OBJECT_ID_ROOM, 0, canvasId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::getSelectedAvatarId
    public int GetSelectedAvatarId()
    {
        if (_eventHandler != null)
        {
            return _eventHandler.GetSelectedAvatarId();
        }

        return -1;
    }

    /// @see com.sulake.habbo.room.RoomEngine::showUseProductSelection
    public void ShowUseProductSelection(int objectId, int category, int targetCategory = -1)
    {
        // TODO: Wire when pet product menu widget is ported
    }

    /// @see com.sulake.habbo.room.RoomEngine::setAvatarEffect
    public void SetAvatarEffect(int effectId)
    {
        // TODO: Wire when _sessionDataManager and _roomSessionManager are ported
        // AS3: gets session from _roomSessionManager, then calls updateObjectUserEffect
    }

    /// @see com.sulake.habbo.room.RoomEngine::setTileCursorState
    public void SetTileCursorState(int roomId, int state)
    {
        IRoomObjectController? cursor = GetTileCursor(roomId);

        if (cursor?.EventHandler != null)
        {
            cursor.EventHandler.ProcessUpdateMessage(new RoomObjectDataUpdateMessage(state, null));
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::toggleTileCursorVisibility
    public void ToggleTileCursorVisibility(int roomId, bool visible)
    {
        IRoomObjectController? cursor = GetTileCursor(roomId);

        if (cursor?.EventHandler != null)
        {
            cursor.EventHandler.ProcessUpdateMessage(
                new RoomObjectTileCursorUpdateMessage(null, 0, visible, "", true));
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::addObjectUpdateCategory
    public void AddObjectUpdateCategory(int category)
    {
        _roomManager?.AddObjectUpdateCategory(category);
    }

    /// @see com.sulake.habbo.room.RoomEngine::removeObjectUpdateCategory
    public void RemoveObjectUpdateCategory(int category)
    {
        _roomManager?.RemoveObjectUpdateCategory(category);
    }

    /// @see com.sulake.habbo.room.RoomEngine::snapshotRoomCanvasToBitmap
    public bool SnapshotRoomCanvasToBitmap(int roomId, int canvasId, Image data, Transform2D transform, bool clip)
    {
        // TODO: Implement (Phase 13+)
        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::runUpdate
    public void RunUpdate()
    {
        Update((uint)System.Environment.TickCount64);
    }

    /// @see com.sulake.habbo.room.RoomEngine::createScreenShot
    public void CreateScreenShot(int roomId, int canvasId, string format)
    {
        // TODO: Implement (Phase 13+)
    }

    /// @see com.sulake.habbo.room.RoomEngine::purgeRoomContent
    public void PurgeRoomContent()
    {
        _roomContentLoader?.Purge();
    }

    /// @see com.sulake.habbo.room.RoomEngine::setMoveBlocked
    public void SetMoveBlocked(bool blocked)
    {
        _isMoveBlocked = blocked;
    }
    #endregion

    #region IRoomEngineServices Remaining
    /// @see com.sulake.habbo.room.RoomEngine::updateObjectRoomWindow
    public void UpdateObjectRoomWindow(int roomId, int objectId, bool update = true)
    {
        string maskId = "20_" + objectId;
        RoomObjectRoomMaskUpdateMessage? msg = null;

        IRoomObjectController? wallItem = GetObjectWallItem(roomId, objectId);

        if (wallItem?.Model != null)
        {
            if (wallItem.Model.GetNumber("furniture_uses_plane_mask") > 0)
            {
                string maskType = wallItem.Model.GetString("furniture_plane_mask_type") ?? "";
                IVector3d location = wallItem.Location;

                if (update)
                {
                    msg = new RoomObjectRoomMaskUpdateMessage(
                        RoomObjectRoomMaskUpdateMessage.ADD_MASK, maskId, maskType, location);
                }
                else
                {
                    msg = new RoomObjectRoomMaskUpdateMessage(
                        RoomObjectRoomMaskUpdateMessage.REMOVE_MASK, maskId);
                }
            }
        }
        else
        {
            msg = new RoomObjectRoomMaskUpdateMessage(
                RoomObjectRoomMaskUpdateMessage.REMOVE_MASK, maskId);
        }

        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler != null && msg != null)
        {
            roomObj.EventHandler.ProcessUpdateMessage(msg);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::setObjectMoverIconSprite
    public void SetObjectMoverIconSprite(int objectId, int category, bool isWallItem,
        string? type = null, IStuffData? stuffData = null, int colorIndex = -1,
        int typeId = -1, string? instanceData = null)
    {
        // TODO: Implement (Phase 13+)
    }

    /// @see com.sulake.habbo.room.RoomEngine::setObjectMoverIconSpriteVisible
    public void SetObjectMoverIconSpriteVisible(bool visible)
    {
        // TODO: Implement (Phase 13+)
    }

    /// @see com.sulake.habbo.room.RoomEngine::removeObjectMoverIconSprite
    public void RemoveObjectMoverIconSprite()
    {
        // TODO: Implement (Phase 13+)
    }

    /// @see com.sulake.habbo.room.RoomEngine::getSelectionArrow
    public IRoomObjectController? GetSelectionArrow(int roomId)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.GetObject(OBJECT_ID_SELECTION_ARROW, 200) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::getTileCursor
    public IRoomObjectController? GetTileCursor(int roomId)
    {
        IRoomInstance? room = GetRoom(roomId);
        return room?.GetObject(OBJECT_ID_ROOM_HIGHLIGHTER, 200) as IRoomObjectController;
    }

    /// @see com.sulake.habbo.room.RoomEngine::requestRoomAdImage
    public void RequestRoomAdImage(int roomId, int objectId, int category, string imageUrl, string type)
    {
        // TODO: Implement (Phase 13+)
    }

    /// @see com.sulake.habbo.room.RoomEngine::requestMouseCursor
    public void RequestMouseCursor(string type, int objectId, string cursor)
    {
        int category = GetRoomObjectCategory(cursor);
        string owner = category + "_" + objectId;
        RoomInstanceData? data = GetRoomInstanceData(_activeRoomId);

        if (data == null)
        {
            return;
        }

        if (type == "ROFCAE_MOUSE_BUTTON")
        {
            if (IsGameMode && category == RoomObjectCategoryEnum.OBJECT_CATEGORY_USER)
            {
                PlayerUnderCursor = objectId;
            }

            data.AddButtonMouseCursorOwner(owner);
        }
        else
        {
            if (IsGameMode && category == RoomObjectCategoryEnum.OBJECT_CATEGORY_USER)
            {
                PlayerUnderCursor = -1;
            }

            data.RemoveButtonMouseCursorOwner(owner);
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::addFloorHole
    public void AddFloorHole(int roomId, int objectId)
    {
        if (objectId < 0)
        {
            return;
        }

        IRoomObjectController? roomObj = GetObjectRoom(roomId);
        IRoomObjectController? furniObj = GetObjectFurniture(roomId, objectId);

        if (furniObj?.Model != null && roomObj?.EventHandler != null)
        {
            int x = (int)furniObj.Location.X;
            int y = (int)furniObj.Location.Y;
            int sizeX = (int)furniObj.Model.GetNumber("furniture_size_x");
            int sizeY = (int)furniObj.Model.GetNumber("furniture_size_y");

            roomObj.EventHandler.ProcessUpdateMessage(
                new RoomObjectRoomFloorHoleUpdateMessage(
                    RoomObjectRoomFloorHoleUpdateMessage.ADD_HOLE, objectId, x, y, sizeX, sizeY));
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::removeFloorHole
    public void RemoveFloorHole(int roomId, int objectId)
    {
        if (objectId < 0)
        {
            return;
        }

        IRoomObjectController? roomObj = GetObjectRoom(roomId);

        if (roomObj?.EventHandler != null)
        {
            roomObj.EventHandler.ProcessUpdateMessage(
                new RoomObjectRoomFloorHoleUpdateMessage(
                    RoomObjectRoomFloorHoleUpdateMessage.REMOVE_HOLE, objectId));
        }
    }

    /// @see com.sulake.habbo.room.RoomEngine::getActiveRoomActiveCanvas
    public IRoomRenderingCanvas? GetActiveRoomActiveCanvas()
    {
        return GetRoomCanvas(_activeRoomId, _activeCanvasId);
    }

    /// @see com.sulake.habbo.room.RoomEngine::requestBadgeImageAsset
    public void RequestBadgeImageAsset(int roomId, int objectId, int category, string badgeId, bool isGroupBadge = true)
    {
        // TODO: Wire when badge image system is ported
    }

    /// @see com.sulake.habbo.room.RoomEngine::isAreaSelectionMode
    public bool IsAreaSelectionMode()
    {
        // TODO: Implement when RoomAreaSelectionManager is wired
        return false;
    }

    /// @see com.sulake.habbo.room.RoomEngine::isMoveBlocked
    public bool IsMoveBlocked()
    {
        return _isMoveBlocked;
    }

    /// @see com.sulake.habbo.room.RoomEngine::isWhereYouClickWhereYouGo
    public bool IsWhereYouClickWhereYouGo()
    {
        return _whereYouClickIsWhereYouGo;
    }

    /// @see com.sulake.habbo.room.RoomEngine::cameraFollowDuration
    private int CameraFollowDuration => GetBoolean("room.camera.follow_user") ? 1000 : 0;
    #endregion
}
