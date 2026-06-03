using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Communication.Messages;
using Vortex.Room.Object;
using Vortex.Room.Utils;

using Vortex.Habbo.Communication.Messages.Incoming.Handshake;
using Vortex.Habbo.Communication.Messages.Incoming.Help;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Action;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Chat;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Engine;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Layout;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Pets;
using Vortex.Habbo.Communication.Messages.Incoming.Room.Session;
using Vortex.Habbo.Communication.Messages.Incoming.Users;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;
using Vortex.Habbo.Communication.Messages.Parser.Handshake;
using Vortex.Habbo.Communication.Messages.Parser.Help;
using Vortex.Habbo.Communication.Messages.Parser.Room.Action;
using Vortex.Habbo.Communication.Messages.Parser.Room.Chat;
using Vortex.Habbo.Communication.Messages.Parser.Room.Engine;
using Vortex.Habbo.Communication.Messages.Parser.Room.Furniture;
using Vortex.Habbo.Communication.Messages.Parser.Room.Layout;
using Vortex.Habbo.Communication.Messages.Parser.Room.Pets;
using Vortex.Habbo.Communication.Messages.Parser.Room.Session;
using Vortex.Habbo.Communication.Messages.Parser.Users;
using Vortex.Habbo.Room.Object;
using Vortex.Habbo.Room.Object.Data;
using Vortex.Habbo.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.RoomMessageHandler
public class RoomMessageHandler : IDisposable
{
    public const int EFFECT_ROOM_ROTATE = 0;
    public const int EFFECT_ROOM_SHAKE = 1;
    public const int EFFECT_ROOM_ZOOM = 2;
    public const int EFFECT_ROOM_DISCO = 3;

    private IConnection? _connection;
    private IRoomCreator? _roomCreator;
    private RoomPlaneParser? _planeParser;
    private RoomEntryTileMessageEvent? _entryTileEvent;
    private int _ownUserId = -1;
    private bool _initialConnection = true;
    private int _currentRoomId;
    private int _guideUserId = -1;
    private int _requesterUserId = -1;

    // Registered message events
    private RoomReadyMessageEvent? _roomReadyEvent;
    private RoomPropertyMessageEvent? _roomPropertyEvent;
    private RoomEntryTileMessageEvent? _roomEntryTileEvent;
    private FloorHeightMapMessageEvent? _floorHeightMapEvent;
    private HeightMapMessageEvent? _heightMapEvent;
    private HeightMapUpdateMessageEvent? _heightMapUpdateEvent;
    private RoomVisualizationSettingsEvent? _roomVisualizationSettingsEvent;
    private FurnitureAliasesMessageEvent? _furnitureAliasesEvent;
    private ObjectsMessageEvent? _objectsEvent;
    private ObjectAddMessageEvent? _objectAddEvent;
    private ObjectUpdateMessageEvent? _objectUpdateEvent;
    private ObjectDataUpdateMessageEvent? _objectDataUpdateEvent;
    private ObjectsDataUpdateMessageEvent? _objectsDataUpdateEvent;
    private ObjectRemoveMessageEvent? _objectRemoveEvent;
    private ObjectRemoveMultipleMessageEvent? _objectRemoveMultipleEvent;
    private ItemsMessageEvent? _itemsEvent;
    private ItemAddMessageEvent? _itemAddEvent;
    private ItemRemoveMessageEvent? _itemRemoveEvent;
    private ItemUpdateMessageEvent? _itemUpdateEvent;
    private ItemStateUpdateMessageEvent? _itemStateUpdateEvent;
    private ItemsStateUpdateMessageEvent? _itemsStateUpdateEvent;
    private ItemDataUpdateMessageEvent? _itemDataUpdateEvent;
    private UsersMessageEvent? _usersEvent;
    private UserUpdateMessageEvent? _userUpdateEvent;
    private UserRemoveMessageEvent? _userRemoveEvent;
    private UserChangeMessageEvent? _userChangeEvent;
    private ExpressionMessageEvent? _expressionEvent;
    private DanceMessageEvent? _danceEvent;
    private AvatarEffectMessageEvent? _avatarEffectEvent;
    private SleepMessageEvent? _sleepEvent;
    private CarryObjectMessageEvent? _carryObjectEvent;
    private UseObjectMessageEvent? _useObjectEvent;
    private SlideObjectBundleMessageEvent? _slideObjectBundleEvent;
    private WiredMovementsMessageEvent? _wiredMovementsEvent;
    private ChatMessageEvent? _chatEvent;
    private WhisperMessageEvent? _whisperEvent;
    private ShoutMessageEvent? _shoutEvent;
    private UserTypingMessageEvent? _userTypingEvent;
    private DiceValueMessageEvent? _diceValueEvent;
    private OneWayDoorStatusMessageEvent? _oneWayDoorStatusEvent;
    private YouArePlayingGameMessageEvent? _youArePlayingGameEvent;
    private YouAreNotSpectatorMessageEvent? _youAreNotSpectatorEvent;
    private GamePlayerValueMessageEvent? _gamePlayerValueEvent;
    private HanditemConfigurationMessageEvent? _handitemConfigurationEvent;
    private SpecialRoomEffectMessageEvent? _specialRoomEffectEvent;
    private BuildersClubPlacementWarningMessageEvent? _buildersClubPlacementWarningEvent;
    private ObjectRemoveConfirmMessageEvent? _objectRemoveConfirmEvent;
    private UserObjectEvent? _userObjectEvent;
    private PetExperienceEvent? _petExperienceEvent;
    private PetFigureUpdateEvent? _petFigureUpdateEvent;
    private IgnoreResultMessageEvent? _ignoreResultEvent;
    private GuideSessionStartedMessageEvent? _guideSessionStartedEvent;
    private GuideSessionEndedMessageEvent? _guideSessionEndedEvent;
    private GuideSessionErrorMessageEvent? _guideSessionErrorEvent;

    /// @see com.sulake.habbo.room.RoomMessageHandler::RoomMessageHandler
    public RoomMessageHandler(IRoomCreator roomCreator)
    {
        _roomCreator = roomCreator;
        _planeParser = new RoomPlaneParser();
        _initialConnection = true;
    }

    #region Connection

    /// @see com.sulake.habbo.room.RoomMessageHandler::set connection
    public IConnection? Connection
    {
        set
        {
            if (_connection != null)
            {
                return;
            }

            _connection = value;

            if (_connection == null)
            {
                return;
            }

            _userObjectEvent = new UserObjectEvent(OnOwnUserEvent);
            _connection.AddMessageEvent(_userObjectEvent);

            _roomReadyEvent = new RoomReadyMessageEvent(OnRoomReady);
            _connection.AddMessageEvent(_roomReadyEvent);

            _roomPropertyEvent = new RoomPropertyMessageEvent(OnRoomProperty);
            _connection.AddMessageEvent(_roomPropertyEvent);

            _roomEntryTileEvent = new RoomEntryTileMessageEvent(OnEntryTileData);
            _connection.AddMessageEvent(_roomEntryTileEvent);

            _floorHeightMapEvent = new FloorHeightMapMessageEvent(OnFloorHeightMap);
            _connection.AddMessageEvent(_floorHeightMapEvent);

            _heightMapEvent = new HeightMapMessageEvent(OnHeightMap);
            _connection.AddMessageEvent(_heightMapEvent);

            _heightMapUpdateEvent = new HeightMapUpdateMessageEvent(OnHeightMapUpdate);
            _connection.AddMessageEvent(_heightMapUpdateEvent);

            _roomVisualizationSettingsEvent = new RoomVisualizationSettingsEvent(OnRoomVisualizationSettings);
            _connection.AddMessageEvent(_roomVisualizationSettingsEvent);

            _furnitureAliasesEvent = new FurnitureAliasesMessageEvent(OnFurnitureAliases);
            _connection.AddMessageEvent(_furnitureAliasesEvent);

            // TODO: AreaHideMessageEvent — unported
            // _areaHideEvent = new AreaHideMessageEvent(OnAreaHide);
            // _connection.AddMessageEvent(_areaHideEvent);

            _objectsEvent = new ObjectsMessageEvent(OnObjects);
            _connection.AddMessageEvent(_objectsEvent);

            _objectAddEvent = new ObjectAddMessageEvent(OnObjectAdd);
            _connection.AddMessageEvent(_objectAddEvent);

            _objectUpdateEvent = new ObjectUpdateMessageEvent(OnObjectUpdate);
            _connection.AddMessageEvent(_objectUpdateEvent);

            _objectDataUpdateEvent = new ObjectDataUpdateMessageEvent(OnObjectDataUpdate);
            _connection.AddMessageEvent(_objectDataUpdateEvent);

            _objectsDataUpdateEvent = new ObjectsDataUpdateMessageEvent(OnObjectsDataUpdate);
            _connection.AddMessageEvent(_objectsDataUpdateEvent);

            _objectRemoveEvent = new ObjectRemoveMessageEvent(OnObjectRemove);
            _connection.AddMessageEvent(_objectRemoveEvent);

            _objectRemoveMultipleEvent = new ObjectRemoveMultipleMessageEvent(OnObjectRemoveMultiple);
            _connection.AddMessageEvent(_objectRemoveMultipleEvent);

            _itemsEvent = new ItemsMessageEvent(OnItems);
            _connection.AddMessageEvent(_itemsEvent);

            _itemAddEvent = new ItemAddMessageEvent(OnItemAdd);
            _connection.AddMessageEvent(_itemAddEvent);

            _itemRemoveEvent = new ItemRemoveMessageEvent(OnItemRemove);
            _connection.AddMessageEvent(_itemRemoveEvent);

            _itemUpdateEvent = new ItemUpdateMessageEvent(OnItemUpdate);
            _connection.AddMessageEvent(_itemUpdateEvent);

            _itemStateUpdateEvent = new ItemStateUpdateMessageEvent(OnItemStateUpdate);
            _connection.AddMessageEvent(_itemStateUpdateEvent);

            _itemsStateUpdateEvent = new ItemsStateUpdateMessageEvent(OnItemsStateUpdate);
            _connection.AddMessageEvent(_itemsStateUpdateEvent);

            _itemDataUpdateEvent = new ItemDataUpdateMessageEvent(OnItemDataUpdate);
            _connection.AddMessageEvent(_itemDataUpdateEvent);

            _usersEvent = new UsersMessageEvent(OnUsers);
            _connection.AddMessageEvent(_usersEvent);

            _userUpdateEvent = new UserUpdateMessageEvent(OnUserUpdate);
            _connection.AddMessageEvent(_userUpdateEvent);

            _userRemoveEvent = new UserRemoveMessageEvent(OnUserRemove);
            _connection.AddMessageEvent(_userRemoveEvent);

            _userChangeEvent = new UserChangeMessageEvent(OnUserChange);
            _connection.AddMessageEvent(_userChangeEvent);

            _expressionEvent = new ExpressionMessageEvent(OnExpression);
            _connection.AddMessageEvent(_expressionEvent);

            _danceEvent = new DanceMessageEvent(OnDance);
            _connection.AddMessageEvent(_danceEvent);

            _avatarEffectEvent = new AvatarEffectMessageEvent(OnAvatarEffect);
            _connection.AddMessageEvent(_avatarEffectEvent);

            _sleepEvent = new SleepMessageEvent(OnAvatarSleep);
            _connection.AddMessageEvent(_sleepEvent);

            _carryObjectEvent = new CarryObjectMessageEvent(OnCarryObject);
            _connection.AddMessageEvent(_carryObjectEvent);

            _useObjectEvent = new UseObjectMessageEvent(OnUseObject);
            _connection.AddMessageEvent(_useObjectEvent);

            _slideObjectBundleEvent = new SlideObjectBundleMessageEvent(OnSlideUpdate);
            _connection.AddMessageEvent(_slideObjectBundleEvent);

            _wiredMovementsEvent = new WiredMovementsMessageEvent(OnWiredMovements);
            _connection.AddMessageEvent(_wiredMovementsEvent);

            _chatEvent = new ChatMessageEvent(OnChat);
            _connection.AddMessageEvent(_chatEvent);

            _whisperEvent = new WhisperMessageEvent(OnChat);
            _connection.AddMessageEvent(_whisperEvent);

            _shoutEvent = new ShoutMessageEvent(OnChat);
            _connection.AddMessageEvent(_shoutEvent);

            _userTypingEvent = new UserTypingMessageEvent(OnTypingStatus);
            _connection.AddMessageEvent(_userTypingEvent);

            _diceValueEvent = new DiceValueMessageEvent(OnDiceValue);
            _connection.AddMessageEvent(_diceValueEvent);

            _oneWayDoorStatusEvent = new OneWayDoorStatusMessageEvent(OnOneWayDoorStatus);
            _connection.AddMessageEvent(_oneWayDoorStatusEvent);

            _petExperienceEvent = new PetExperienceEvent(OnPetExperience);
            _connection.AddMessageEvent(_petExperienceEvent);

            _petFigureUpdateEvent = new PetFigureUpdateEvent(OnPetFigureUpdate);
            _connection.AddMessageEvent(_petFigureUpdateEvent);

            _youArePlayingGameEvent = new YouArePlayingGameMessageEvent(OnPlayingGame);
            _connection.AddMessageEvent(_youArePlayingGameEvent);

            _youAreNotSpectatorEvent = new YouAreNotSpectatorMessageEvent(OnYouAreNotSpectator);
            _connection.AddMessageEvent(_youAreNotSpectatorEvent);

            _gamePlayerValueEvent = new GamePlayerValueMessageEvent(OnGamePlayerNumberValue);
            _connection.AddMessageEvent(_gamePlayerValueEvent);

            _handitemConfigurationEvent = new HanditemConfigurationMessageEvent(OnHanditemConfiguration);
            _connection.AddMessageEvent(_handitemConfigurationEvent);

            _ignoreResultEvent = new IgnoreResultMessageEvent(OnIgnoreResult);
            _connection.AddMessageEvent(_ignoreResultEvent);

            _guideSessionStartedEvent = new GuideSessionStartedMessageEvent(OnGuideSessionStarted);
            _connection.AddMessageEvent(_guideSessionStartedEvent);

            _guideSessionEndedEvent = new GuideSessionEndedMessageEvent(OnGuideSessionEnded);
            _connection.AddMessageEvent(_guideSessionEndedEvent);

            _guideSessionErrorEvent = new GuideSessionErrorMessageEvent(OnGuideSessionError);
            _connection.AddMessageEvent(_guideSessionErrorEvent);

            _specialRoomEffectEvent = new SpecialRoomEffectMessageEvent(OnSpecialRoomEvent);
            _connection.AddMessageEvent(_specialRoomEffectEvent);

            _buildersClubPlacementWarningEvent = new BuildersClubPlacementWarningMessageEvent(OnBCPlacementWarning);
            _connection.AddMessageEvent(_buildersClubPlacementWarningEvent);

            _objectRemoveConfirmEvent = new ObjectRemoveConfirmMessageEvent(OnObjectRemoveConfirm);
            _connection.AddMessageEvent(_objectRemoveConfirmEvent);
        }
    }

    #endregion

    #region Lifecycle

    /// @see com.sulake.habbo.room.RoomMessageHandler::dispose
    public void Dispose()
    {
        _connection = null;
        _roomCreator = null;

        if (_planeParser != null)
        {
            _planeParser.Dispose();
            _planeParser = null;
        }

        _entryTileEvent = null;
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::setCurrentRoom
    public void SetCurrentRoom(int roomId)
    {
        if (_currentRoomId != 0)
        {
            _roomCreator?.DisposeRoom(_currentRoomId);
        }

        _currentRoomId = roomId;
        _entryTileEvent = null;
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::resetCurrentRoom
    public void ResetCurrentRoom()
    {
        _currentRoomId = 0;
        _entryTileEvent = null;
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::getRoomId
    private int GetRoomId(int unused = 0)
    {
        return _currentRoomId;
    }

    #endregion

    #region Room Setup Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onRoomReady
    private void OnRoomReady(IMessageEvent e)
    {
        if (_roomCreator == null || _connection == null)
        {
            return;
        }

        var parser = e.parser as RoomReadyMessageEventParser;
        if (parser == null)
        {
            return;
        }

        if (_currentRoomId != parser.RoomId)
        {
            SetCurrentRoom(parser.RoomId);
        }

        _roomCreator.SetWorldType(_currentRoomId, parser.RoomType);

        if (_initialConnection)
        {
            _connection.Send(new GetFurnitureAliasesMessageComposer());
            _initialConnection = false;
        }
        else
        {
            _connection.Send(new GetHeightMapMessageComposer());
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onFurnitureAliases
    private void OnFurnitureAliases(IMessageEvent e)
    {
        if (_roomCreator == null || _connection == null)
        {
            return;
        }

        var parser = e.parser as FurnitureAliasesMessageEventParser;
        if (parser == null)
        {
            return;
        }

        for (int i = 0; i < parser.AliasCount; i++)
        {
            string? name = parser.GetName(i);
            string? alias = parser.GetAlias(i);

            if (name != null && alias != null)
            {
                _roomCreator.SetRoomObjectAlias(name, alias);
            }
        }

        _connection.Send(new GetHeightMapMessageComposer());
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onEntryTileData
    private void OnEntryTileData(IMessageEvent e)
    {
        if (e is RoomEntryTileMessageEvent entryEvent)
        {
            _entryTileEvent = entryEvent;
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onHeightMap
    private void OnHeightMap(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as HeightMapMessageEventParser;
        if (parser == null)
        {
            return;
        }

        int width = parser.Width;
        int height = parser.Height;

        int stackingBlockedMaskBit = _roomCreator.Configuration != null
            && _roomCreator.Configuration.PropertyExists("room.stacking_blocked_mask_bit")
            ? _roomCreator.Configuration.GetInteger("room.stacking_blocked_mask_bit", 14)
            : 14;

        parser.StackingBlockedMaskBit = stackingBlockedMaskBit;

        FurniStackingHeightMap heightMap = new(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heightMap.SetTileHeight(x, y, parser.GetTileHeight(x, y));
                heightMap.SetStackingBlocked(x, y, parser.GetStackingBlocked(x, y));
                heightMap.SetIsRoomTile(x, y, parser.IsRoomTile(x, y));
            }
        }

        _roomCreator.SetFurniStackingHeightMap(_currentRoomId, heightMap);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onHeightMapUpdate
    private void OnHeightMapUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as HeightMapUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        FurniStackingHeightMap? heightMap = _roomCreator.GetFurniStackingHeightMap(_currentRoomId);
        if (heightMap == null)
        {
            return;
        }

        int stackingBlockedMaskBit = _roomCreator.Configuration != null
            && _roomCreator.Configuration.PropertyExists("room.stacking_blocked_mask_bit")
            ? _roomCreator.Configuration.GetInteger("room.stacking_blocked_mask_bit", 14)
            : 14;

        parser.StackingBlockedMaskBit = stackingBlockedMaskBit;

        // Iterator pattern — parser.Next() returns true while there are more updates
        while (parser.Next())
        {
            int x = parser.X;
            int y = parser.Y;

            heightMap.SetTileHeight(x, y, parser.TileHeight);
            heightMap.SetStackingBlocked(x, y, parser.IsStackingBlocked);
            heightMap.SetIsRoomTile(x, y, parser.IsRoomTile);
        }

        _roomCreator.RefreshTileObjectMap(_currentRoomId, "RoomMessageHandler.OnHeightMapUpdate()");
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onFloorHeightMap
    private void OnFloorHeightMap(IMessageEvent e)
    {
        if (_roomCreator == null || _planeParser == null)
        {
            return;
        }

        var parser = e.parser as FloorHeightMapMessageEventParser;
        if (parser == null)
        {
            return;
        }

        LegacyWallGeometry? geometry = _roomCreator.GetLegacyGeometry(_currentRoomId);
        if (geometry == null)
        {
            return;
        }

        _planeParser.Reset();

        int width = parser.Width;
        int height = parser.Height;

        _planeParser.InitializeTileMap(width, height);

        // Door detection variables
        double doorX = -1;
        double doorY = -1;
        double doorZ = 0;
        double doorDir = 0;

        // Get entry tile data if available
        RoomEntryTileMessageEventParser? entryParser = _entryTileEvent?.parser as RoomEntryTileMessageEventParser;

        FurniStackingHeightMap? stackMap = _roomCreator.GetFurniStackingHeightMap(_currentRoomId);
        if (stackMap == null)
        {
            return;
        }

        // Build tile map and detect door
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tileHeight = parser.GetTileHeight(x, y);

                if (x > 0 && x < width - 1 && y > 0 && y < height - 1
                    && tileHeight != RoomPlaneParser.TILE_BLOCKED)
                {
                    bool matchesEntryTile = entryParser == null
                        || (x == entryParser.X && y == entryParser.Y);

                    if (matchesEntryTile)
                    {
                        // Check east-facing door pattern: top/left/bottom all blocked
                        if (parser.GetTileHeight(x, y - 1) == RoomPlaneParser.TILE_BLOCKED
                            && parser.GetTileHeight(x - 1, y) == RoomPlaneParser.TILE_BLOCKED
                            && parser.GetTileHeight(x, y + 1) == RoomPlaneParser.TILE_BLOCKED)
                        {
                            doorX = x + 0.5;
                            doorY = y;
                            doorZ = tileHeight;
                            doorDir = 90;
                        }
                        // Check south-facing door pattern: top/left/right all blocked
                        else if (parser.GetTileHeight(x, y - 1) == RoomPlaneParser.TILE_BLOCKED
                            && parser.GetTileHeight(x - 1, y) == RoomPlaneParser.TILE_BLOCKED
                            && parser.GetTileHeight(x + 1, y) == RoomPlaneParser.TILE_BLOCKED)
                        {
                            doorX = x;
                            doorY = y + 0.5;
                            doorZ = tileHeight;
                            doorDir = 180;
                        }
                    }
                }

                _planeParser.SetTileHeight(x, y, tileHeight);
            }
        }

        // Initialize planes with door correction
        int doorTileX = (int)Math.Floor(doorX);
        int doorTileY = (int)Math.Floor(doorY);

        if (doorTileX >= 0 && doorTileY >= 0)
        {
            _planeParser.SetTileHeight(doorTileX, doorTileY, doorZ);
        }

        _planeParser.InitializeFromTileData(parser.FixedWallsHeight);

        double wallHeight = _planeParser.FloorHeight;

        if (doorTileX >= 0 && doorTileY >= 0)
        {
            _planeParser.SetTileHeight(doorTileX, doorTileY, doorZ + wallHeight);
        }

        // Initialize legacy geometry
        geometry.Scale = (int)parser.Scale;
        geometry.Initialize(width, height, (int)wallHeight);

        // Fill legacy geometry with tile heights (reverse iterate per AS3)
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = width - 1; x >= 0; x--)
            {
                double planeHeight = _planeParser.GetTileHeight(x, y);
                geometry.SetTileHeight(x, y, Math.Abs(planeHeight));
            }
        }

        // Build room XML with door data
        XElement doorsElement = new("doors",
            new XElement("door",
                new XAttribute("x", doorX),
                new XAttribute("y", doorY),
                new XAttribute("z", doorZ),
                new XAttribute("dir", doorDir)));

        XElement roomXml = _planeParser.GetXML();
        roomXml.Add(doorsElement);

        _roomCreator.InitializeRoom(_currentRoomId, roomXml);

        // Process area hide data
        if (parser.AreaHideData != null)
        {
            foreach (var hideData in parser.AreaHideData)
            {
                _roomCreator.UpdateAreaHide(_currentRoomId, hideData.Id, hideData.IsHidden,
                    hideData.RootX, hideData.RootY, hideData.Width, hideData.Height, hideData.InvertArea);
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onRoomVisualizationSettings
    private void OnRoomVisualizationSettings(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as RoomVisualizationSettingsEventParser;
        if (parser == null)
        {
            return;
        }

        bool wallsVisible = !parser.WallsHidden;
        bool floorsVisible = true;

        _roomCreator.UpdateObjectRoomVisibilities(_currentRoomId, wallsVisible, floorsVisible);
        _roomCreator.UpdateObjectRoomPlaneThicknesses(_currentRoomId,
            parser.WallThicknessMultiplier, parser.FloorThicknessMultiplier);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onRoomProperty
    private void OnRoomProperty(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as RoomPropertyMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectRoom(_currentRoomId,
            parser.FloorType, parser.WallType, parser.LandscapeType);
    }

    #endregion

    #region Object Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjects
    private void OnObjects(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectsMessageEventParser;
        if (parser == null)
        {
            return;
        }

        for (int i = 0; i < parser.ObjectCount; i++)
        {
            ObjectMessageData? obj = parser.GetObject(i);
            if (obj != null)
            {
                AddActiveObject(_currentRoomId, obj);
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectAdd
    private void OnObjectAdd(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectAddMessageEventParser;
        if (parser?.Data == null)
        {
            return;
        }

        AddActiveObject(_currentRoomId, parser.Data);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectUpdate
    private void OnObjectUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectUpdateMessageEventParser;
        ObjectMessageData? data = parser?.Data;
        if (data == null)
        {
            return;
        }

        Vector3d location = new(data.X, data.Y, data.Z);
        Vector3d direction = new(data.Dir);

        _roomCreator.UpdateObjectFurniture(_currentRoomId, data.Id,
            location, direction, data.State, data.Data, data.Extra);
        _roomCreator.UpdateObjectFurnitureHeight(_currentRoomId, data.Id, data.SizeZ);
        _roomCreator.UpdateObjectFurnitureExpiryTime(_currentRoomId, data.Id, data.ExpiryTime);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectDataUpdate
    private void OnObjectDataUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectDataUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectFurniture(_currentRoomId, parser.Id,
            new Vector3d(), new Vector3d(), parser.State, parser.Data);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectsDataUpdate
    private void OnObjectsDataUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectsDataUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        for (int i = 0; i < parser.ObjectCount; i++)
        {
            ObjectDataUpdateMessageData? item = parser.GetObjectData(i);
            if (item != null)
            {
                _roomCreator.UpdateObjectFurniture(_currentRoomId, item.Id,
                    new Vector3d(), new Vector3d(), item.State, item.Data);
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectRemove
    private void OnObjectRemove(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectRemoveMessageEventParser;
        if (parser == null)
        {
            return;
        }

        int id = parser.Id;
        int pickerId = parser.IsExpired ? -1 : parser.PickerId;
        int delay = parser.Delay;

        if (delay > 0)
        {
            IRoomCreator creator = _roomCreator;
            int roomId = _currentRoomId;
            _ = Task.Delay(delay).ContinueWith(_ =>
                creator.DisposeObjectFurniture(roomId, id, pickerId, true));
        }
        else
        {
            _roomCreator.DisposeObjectFurniture(_currentRoomId, id, pickerId, true);
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectRemoveMultiple
    private void OnObjectRemoveMultiple(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ObjectRemoveMultipleMessageEventParser;
        if (parser == null)
        {
            return;
        }

        foreach (int id in parser.Ids)
        {
            _roomCreator.DisposeObjectFurniture(_currentRoomId, id, parser.PickerId);
        }

        _roomCreator.RefreshTileObjectMap(_currentRoomId, "RoomMessageHandler.OnObjectRemoveMultiple()");
    }

    #endregion

    #region Item (Wall) Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItems
    private void OnItems(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemsMessageEventParser;
        if (parser == null)
        {
            return;
        }

        for (int i = 0; i < parser.ItemCount; i++)
        {
            ItemMessageData? item = parser.GetItem(i);
            if (item != null)
            {
                AddWallItem(_currentRoomId, item);
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItemAdd
    private void OnItemAdd(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemAddMessageEventParser;
        if (parser?.Data == null)
        {
            return;
        }

        AddWallItem(_currentRoomId, parser.Data);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItemRemove
    private void OnItemRemove(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemRemoveMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.DisposeObjectWallItem(_currentRoomId, parser.ItemId, parser.PickerId);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItemUpdate
    private void OnItemUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemUpdateMessageEventParser;
        ItemMessageData? data = parser?.Data;
        if (data == null)
        {
            return;
        }

        LegacyWallGeometry? geometry = _roomCreator.GetLegacyGeometry(_currentRoomId);
        if (geometry == null)
        {
            return;
        }

        IVector3d location = geometry.GetLocation(
            data.WallX, data.WallY, data.LocalX, data.LocalY, data.Dir);
        Vector3d direction = new(geometry.GetDirection(data.Dir));

        _roomCreator.UpdateObjectWallItem(_currentRoomId, data.Id,
            location, direction, data.State, data.Data);
        _roomCreator.UpdateObjectWallItemExpiryTime(_currentRoomId, data.Id, data.SecondsToExpiration);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItemStateUpdate
    private void OnItemStateUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemStateUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectWallItemState(_currentRoomId, parser.Id, parser.State, parser.ItemData);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItemsStateUpdate
    private void OnItemsStateUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemsStateUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        for (int i = 0; i < parser.ItemCount; i++)
        {
            ItemStateUpdateMessageData? item = parser.GetItemData(i);
            if (item != null)
            {
                _roomCreator.UpdateObjectWallItemState(_currentRoomId, item.Id, item.State, item.ItemData);
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onItemDataUpdate
    private void OnItemDataUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ItemDataUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectWallItemData(_currentRoomId, parser.Id, parser.ItemData);
    }

    #endregion

    #region User Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onOwnUserEvent
    private void OnOwnUserEvent(IMessageEvent e)
    {
        var parser = e.parser as UserObjectMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _ownUserId = parser.id;
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onPetFigureUpdate
    private void OnPetFigureUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as PetFigureUpdateMessageEventParser;
        if (parser?.figureData == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserFigure(_currentRoomId, parser.roomIndex,
            parser.figureData.figureString, "", "", parser.isRiding);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onUsers
    private void OnUsers(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as UsersMessageEventParser;
        if (parser == null)
        {
            return;
        }

        for (int i = 0; i < parser.UserCount; i++)
        {
            UserMessageData? user = parser.GetUser(i);
            if (user == null)
            {
                continue;
            }

            int roomIndex = user.RoomIndex;
            Vector3d position = new(user.X, user.Y, user.Z);
            Vector3d direction = new(user.Dir * 45);

            _roomCreator.AddObjectUser(_currentRoomId, roomIndex,
                position, direction, user.Dir * 45.0, user.UserType, user.Figure);

            if (user.WebID == _ownUserId)
            {
                _roomCreator.SetOwnUserId(_currentRoomId, roomIndex);
                _roomCreator.UpdateObjectUserOwnUserAvatar(_currentRoomId, roomIndex);
            }

            _roomCreator.UpdateObjectUserFigure(_currentRoomId, roomIndex,
                user.Figure, user.Sex, user.SubType, user.IsRiding);

            // Monster plants with special posture
            if (user.UserType == RoomObjectUserTypes.GetTypeId(RoomObjectUserTypes.PET)
                && _roomCreator.GetPetTypeId(user.Figure) == 16)
            {
                _roomCreator.UpdateObjectUserPosture(_currentRoomId, roomIndex, user.PetPosture);
            }

            if (_roomCreator.Configuration != null
                && _roomCreator.Configuration.GetBoolean("avatar.ignored.bubble.enabled"))
            {
                // TODO: requires sessionDataManager.isIgnored(user.Name) — unported
                // _roomCreator.UpdateObjectUserAction(_currentRoomId, roomIndex, "figure_is_muted",
                //     Convert.ToInt32(_roomCreator.SessionDataManager.IsIgnored(user.Name)));
            }
        }

        UpdateGuideMarker();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onUserUpdate
    private void OnUserUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as UserUpdateMessageEventParser;
        if (parser == null)
        {
            return;
        }

        double roomZScale = _roomCreator.GetRoomNumberValue(_currentRoomId, "room_z_scale");

        for (int i = 0; i < parser.UserUpdateCount; i++)
        {
            UserUpdateMessageData? data = parser.GetUserUpdateData(i);
            if (data == null)
            {
                continue;
            }

            double localZ = data.LocalZ;
            if (roomZScale != 0)
            {
                localZ /= roomZScale;
            }

            Vector3d position = new(data.X, data.Y, data.Z + localZ);
            Vector3d? target = data.IsMoving
                ? new Vector3d(data.TargetX, data.TargetY, data.TargetZ)
                : null;

            _roomCreator.UpdateObjectUser(_currentRoomId, data.Id,
                position, new Vector3d(data.Dir * 45),
                data.CanStandUp, localZ, target,
                data.DirHead * 45.0, double.NaN, data.SkipPositionUpdate);

            // Clear flat control
            _roomCreator.UpdateObjectUserFlatControl(_currentRoomId, data.Id, "");

            // Process actions
            string postureType = "";
            string postureParam = "";
            bool hasPosture = false;
            bool isMoving = false;
            bool isSwimming = false;
            bool updatePosture = true;

            foreach (var action in data.Actions)
            {
                switch (action.ActionType)
                {
                    case "flatctrl":
                        _roomCreator.UpdateObjectUserFlatControl(_currentRoomId, data.Id, action.ActionParameter);
                        break;
                    case "sign":
                        if (data.Actions.Count == 1)
                        {
                            updatePosture = false;
                            if (int.TryParse(action.ActionParameter, out int signValue))
                            {
                                _roomCreator.UpdateObjectUserAction(_currentRoomId, data.Id,
                                    "figure_sign", signValue);
                            }
                        }
                        break;
                    case "gst":
                        if (data.Actions.Count == 1)
                        {
                            updatePosture = false;
                            _roomCreator.UpdateObjectPetGesture(_currentRoomId, data.Id, action.ActionParameter);
                        }
                        break;
                    case "wav":
                    case "mv":
                        isMoving = true;
                        hasPosture = true;
                        break;
                    case "swim":
                        isSwimming = true;
                        hasPosture = true;
                        postureType = "swim";
                        break;
                    case "wf":
                    case "trd":
                        break;
                    default:
                        hasPosture = true;
                        break;
                }

                // AS3 unconditionally overwrites these after each action iteration
                postureType = action.ActionType;
                postureParam = action.ActionParameter;
            }

            // If not moving but swimming, use float posture
            if (!isMoving && isSwimming)
            {
                postureType = "float";
            }

            if (updatePosture)
            {
                if (hasPosture)
                {
                    _roomCreator.UpdateObjectUserPosture(_currentRoomId, data.Id, postureType, postureParam);
                }
                else
                {
                    _roomCreator.UpdateObjectUserPosture(_currentRoomId, data.Id, "std", "");
                }
            }
        }

        UpdateGuideMarker();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onUserRemove
    private void OnUserRemove(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as UserRemoveMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.DisposeObjectUser(_currentRoomId, parser.Id);
        UpdateGuideMarker();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onUserChange
    private void OnUserChange(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as UserChangeMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserFigure(_currentRoomId, parser.Id, parser.Figure, parser.Sex);
    }

    #endregion

    #region User Action Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onPetExperience
    private void OnPetExperience(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as PetExperienceMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.petRoomIndex,
            "figure_gained_experience", parser.gainedExperience);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onExpression
    private void OnExpression(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ExpressionMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId,
            "figure_expression", parser.ExpressionType);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onDance
    private void OnDance(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as DanceMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId,
            "figure_dance", parser.DanceStyle);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onAvatarEffect
    private void OnAvatarEffect(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as AvatarEffectMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserEffect(_currentRoomId, parser.UserId,
            parser.EffectId, parser.DelayMilliSeconds);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onAvatarSleep
    private void OnAvatarSleep(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as SleepMessageEventParser;
        if (parser == null)
        {
            return;
        }

        int sleeping = parser.Sleeping ? 1 : 0;
        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId, "figure_sleep", sleeping);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onCarryObject
    private void OnCarryObject(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as CarryObjectMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId,
            "figure_carry_object", parser.ItemType);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onUseObject
    private void OnUseObject(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as UseObjectMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId,
            "figure_use_object", parser.ItemType);
    }

    #endregion

    #region Chat and Typing

    /// @see com.sulake.habbo.room.RoomMessageHandler::onChat
    private void OnChat(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as ChatMessageEventParser;
        if (parser == null)
        {
            // Try whisper/shout parsers which share the same base shape
            if (e.parser is WhisperMessageEventParser whisperParser)
            {
                // Whisper: skip own user's animation
                _roomCreator.UpdateObjectUserGesture(_currentRoomId, whisperParser.UserId, whisperParser.Gesture);
                _roomCreator.UpdateObjectUserAction(_currentRoomId, whisperParser.UserId,
                    "figure_talk", (int)Math.Ceiling(whisperParser.Text.Length / 10.0));
                return;
            }

            if (e.parser is ShoutMessageEventParser shoutParser)
            {
                _roomCreator.UpdateObjectUserGesture(_currentRoomId, shoutParser.UserId, shoutParser.Gesture);
                _roomCreator.UpdateObjectUserAction(_currentRoomId, shoutParser.UserId,
                    "figure_talk", (int)Math.Ceiling(shoutParser.Text.Length / 10.0));
                return;
            }

            return;
        }

        _roomCreator.UpdateObjectUserGesture(_currentRoomId, parser.UserId, parser.Gesture);
        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId,
            "figure_talk", (int)Math.Ceiling(parser.Text.Length / 10.0));
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onTypingStatus
    private void OnTypingStatus(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as UserTypingMessageEventParser;
        if (parser == null)
        {
            return;
        }

        int typing = parser.IsTyping ? 1 : 0;
        _roomCreator.UpdateObjectUserAction(_currentRoomId, parser.UserId, "figure_is_typing", typing);
    }

    #endregion

    #region Slide and Wired Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onSlideUpdate
    private void OnSlideUpdate(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as SlideObjectBundleMessageEventParser;
        if (parser == null)
        {
            return;
        }

        // Double-pulse state update per AS3
        _roomCreator.UpdateObjectFurniture(_currentRoomId, parser.Id,
            new Vector3d(), new Vector3d(), 1, new LegacyStuffData());
        _roomCreator.UpdateObjectFurniture(_currentRoomId, parser.Id,
            new Vector3d(), new Vector3d(), 2, new LegacyStuffData());

        foreach (var obj in parser.ObjectList)
        {
            _roomCreator.UpdateObjectFurnitureLocation(_currentRoomId, obj.Id,
                obj.Loc, null, obj.Target);
        }

        if (parser.Avatar != null)
        {
            var avatar = parser.Avatar;
            _roomCreator.UpdateObjectUser(_currentRoomId, avatar.Id,
                avatar.Loc, new Vector3d(), targetLocation: avatar.Target, isSlide: true);
            SetUserMovePosture(avatar.Id, avatar.MoveType ?? "");
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onWiredMovements
    public void OnWiredMovements(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as WiredMovementsMessageEventParser;
        if (parser == null)
        {
            return;
        }

        foreach (var userMove in parser.UserMoves)
        {
            OnWiredUserMove(userMove);
        }

        foreach (var furniMove in parser.FurniMoves)
        {
            OnWiredFurniMove(furniMove);
        }

        foreach (var wallItemMove in parser.WallItemMoves)
        {
            OnWiredWallItemMove(wallItemMove);
        }

        foreach (var dirUpdate in parser.UserDirectionUpdates)
        {
            OnUserDirectionUpdate(dirUpdate);
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onWiredFurniMove
    private void OnWiredFurniMove(WiredFurniMoveData data)
    {
        if (_roomCreator == null)
        {
            return;
        }

        Vector3d direction = new((int)data.Rotation % 8 * 45);
        _roomCreator.UpdateObjectFurnitureLocation(_currentRoomId, data.FurniId,
            RoundLocation(data.Source), direction, RoundLocation(data.Target), data.AnimationTime);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onWiredWallItemMove
    private void OnWiredWallItemMove(WiredWallItemMoveData data)
    {
        if (_roomCreator == null)
        {
            return;
        }

        LegacyWallGeometry? geometry = _roomCreator.GetLegacyGeometry(_currentRoomId);
        if (geometry == null)
        {
            return;
        }

        string wallDir = data.IsDirectionRight ? "r" : "l";
        IVector3d src = geometry.GetLocation(data.OldWallX, data.OldWallY,
            data.OldOffsetX, data.OldOffsetY, wallDir);
        IVector3d dst = geometry.GetLocation(data.NewWallX, data.NewWallY,
            data.NewOffsetX, data.NewOffsetY, wallDir);

        _roomCreator.UpdateObjectWallItemLocation(_currentRoomId, data.ItemId,
            RoundLocation(src), null, data.AnimationTime);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onWiredUserMove
    private void OnWiredUserMove(WiredUserMoveData data)
    {
        if (_roomCreator == null)
        {
            return;
        }

        Vector3d bodyDir = new((int)data.BodyDirection % 8 * 45);
        double headDir = (int)data.HeadDirection % 8 * 45.0;

        _roomCreator.UpdateObjectUser(_currentRoomId, data.UserIndex,
            RoundLocation(data.Source), bodyDir,
            headDirection: headDir, targetLocation: RoundLocation(data.Target),
            countdownTime: data.AnimationTime);

        SetUserMovePosture(data.UserIndex, data.MoveType);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onUserDirectionUpdate
    private void OnUserDirectionUpdate(WiredUserDirUpdateData data)
    {
        if (_roomCreator == null)
        {
            return;
        }

        Vector3d bodyDir = new((int)data.BodyDirection % 8 * 45);
        double headDir = (int)data.BodyDirection % 8 * 45.0;

        _roomCreator.UpdateObjectUserDir(_currentRoomId, data.UserIndex, bodyDir, headDir);
    }

    #endregion

    #region Furniture Action Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onDiceValue
    private void OnDiceValue(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as DiceValueMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IStuffData data = new LegacyStuffData();
        _roomCreator.UpdateObjectFurniture(_currentRoomId, parser.Id,
            new Vector3d(), new Vector3d(), parser.Value, data);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onOneWayDoorStatus
    private void OnOneWayDoorStatus(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as OneWayDoorStatusMessageEventParser;
        if (parser == null)
        {
            return;
        }

        IStuffData data = new LegacyStuffData();
        _roomCreator.UpdateObjectFurniture(_currentRoomId, parser.Id,
            new Vector3d(), new Vector3d(), parser.Status, data);
    }

    #endregion

    #region Game and Session Handlers

    /// @see com.sulake.habbo.room.RoomMessageHandler::onPlayingGame
    private void OnPlayingGame(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as YouArePlayingGameMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.SetIsPlayingGame(_currentRoomId, parser.IsPlaying);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onYouAreNotSpectator
    private void OnYouAreNotSpectator(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as YouAreNotSpectatorMessageEventParser;
        if (parser == null)
        {
            return;
        }

        if (parser.FlatId != _currentRoomId)
        {
            return;
        }

        // TODO: Check session.isSpectatorMode when session manager is ported
        _roomCreator.LeaveSpectate();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onHanditemConfiguration
    private void OnHanditemConfiguration(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as HanditemConfigurationMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _roomCreator.SetHanditemControlBlocked(_currentRoomId, parser.IsHanditemControlBlocked);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onGamePlayerNumberValue
    private void OnGamePlayerNumberValue(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as GamePlayerValueMessageEventParser;
        if (parser == null)
        {
            return;
        }

        int roomId = GetRoomId(0);
        _roomCreator.UpdateObjectUserAction(roomId, parser.UserId, "figure_number_value", parser.Value);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onSpecialRoomEvent
    public void OnSpecialRoomEvent(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        var parser = e.parser as SpecialRoomEffectMessageEventParser;
        if (parser == null)
        {
            return;
        }

        switch (parser.EffectId)
        {
            case EFFECT_ROOM_ROTATE:
                // TODO: RoomRotatingEffect.Init(250, 5000); RoomRotatingEffect.TurnVisualizationOn() — unported
                break;
            case EFFECT_ROOM_SHAKE:
                // TODO: RoomShakingEffect.Init(250, 5000); RoomShakingEffect.TurnVisualizationOn() — unported
                break;
            case EFFECT_ROOM_ZOOM:
                // TODO: _roomCreator.RoomSessionManager.Events.DispatchEvent(new RoomEngineZoomEvent(_currentRoomId, -1, true)) — requires session manager
                break;
            case EFFECT_ROOM_DISCO:
                StartDiscoEffect(_currentRoomId);
                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onSpecialRoomEvent (disco branch)
    private void StartDiscoEffect(int roomId)
    {
        if (_roomCreator == null)
        {
            return;
        }

        uint[] discoColours = [29371, 16731195, 16764980, 0x99FF00, 29371, 16731195, 16764980, 0x99FF00, 0];
        IRoomCreator creator = _roomCreator;

        // AS3: new Timer(1000, discoColours.length + 1) — fires 10 times at 1s intervals
        _ = Task.Run(async () =>
        {
            for (int i = 0; i < discoColours.Length + 1; i++)
            {
                await Task.Delay(1000);
                if (i == discoColours.Length)
                {
                    // Last tick: reset background color only (AS3: arrayIndex out-of-bounds → 0, bgOnly=true)
                    creator.UpdateObjectRoomColor(roomId, 0, 176, true);
                }
                else
                {
                    creator.UpdateObjectRoomColor(roomId, discoColours[i], 176, false);
                }
            }
        });
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onIgnoreResult
    private void OnIgnoreResult(IMessageEvent e)
    {
        if (_roomCreator == null)
        {
            return;
        }

        if (_roomCreator.Configuration == null
            || !_roomCreator.Configuration.GetBoolean("avatar.ignored.bubble.enabled"))
        {
            return;
        }

        var parser = e.parser as IgnoreResultMessageEventParser;
        if (parser == null)
        {
            return;
        }

        // TODO: Requires sessionDataManager.GetSession().UserDataManager.GetUserDataByName(name) — unported
        // switch (parser.result)
        // {
        //     case 1:
        //     case 2:
        //         _roomCreator.UpdateObjectUserAction(_currentRoomId, userData.roomObjectId, "figure_is_muted", 1);
        //         break;
        //     case 3:
        //         _roomCreator.UpdateObjectUserAction(_currentRoomId, userData.roomObjectId, "figure_is_muted", 0);
        //         break;
        // }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onGuideSessionStarted
    private void OnGuideSessionStarted(IMessageEvent e)
    {
        var parser = e.parser as GuideSessionStartedMessageEventParser;
        if (parser == null)
        {
            return;
        }

        _guideUserId = parser.guideUserId;
        _requesterUserId = parser.requesterUserId;
        UpdateGuideMarker();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onGuideSessionEnded
    private void OnGuideSessionEnded(IMessageEvent e)
    {
        RemoveGuideMarker();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onGuideSessionError
    private void OnGuideSessionError(IMessageEvent e)
    {
        RemoveGuideMarker();
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onBCPlacementWarning
    private void OnBCPlacementWarning(IMessageEvent e)
    {
        if (_roomCreator == null || _connection == null)
        {
            return;
        }

        var parser = e.parser as BuildersClubPlacementWarningMessageEventParser;
        if (parser == null)
        {
            return;
        }

        // TODO: Show confirmation dialog and send placement composer when dialog system is ported
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::onObjectRemoveConfirm
    private void OnObjectRemoveConfirm(IMessageEvent e)
    {
        if (_roomCreator == null || _connection == null)
        {
            return;
        }

        var parser = e.parser as ObjectRemoveConfirmMessageEventParser;
        if (parser == null)
        {
            return;
        }

        // TODO: Show confirmation dialog and send PickupObjectMessageComposer when dialog system is ported
    }

    #endregion

    #region Helper Methods

    /// @see com.sulake.habbo.room.RoomMessageHandler::addActiveObject
    private void AddActiveObject(int roomId, ObjectMessageData data)
    {
        if (_roomCreator == null)
        {
            return;
        }

        if (data.StaticClass != null)
        {
            _roomCreator.AddObjectFurnitureByName(roomId, data.Id, data.StaticClass,
                new Vector3d(data.X, data.Y, data.Z), new Vector3d(data.Dir),
                data.State, data.Data, data.Extra);
        }
        else
        {
            _roomCreator.AddObjectFurniture(roomId, data.Id, data.Type,
                new Vector3d(data.X, data.Y, data.Z), new Vector3d(data.Dir),
                data.State, data.Data, data.Extra, data.ExpiryTime, data.UsagePolicy,
                data.OwnerId, data.OwnerName, true, true, data.SizeZ);
        }
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::addWallItem
    private void AddWallItem(int roomId, ItemMessageData data)
    {
        if (_roomCreator == null)
        {
            return;
        }

        LegacyWallGeometry? geometry = _roomCreator.GetLegacyGeometry(roomId);
        if (geometry == null)
        {
            return;
        }

        IVector3d location;
        if (!data.IsOldFormat)
        {
            location = geometry.GetLocation(data.WallX, data.WallY, data.LocalX, data.LocalY, data.Dir);
        }
        else
        {
            location = geometry.GetLocationOldFormat(data.Y, data.Z, data.Dir);
        }

        Vector3d direction = new(geometry.GetDirection(data.Dir));

        _roomCreator.AddObjectWallItem(roomId, data.Id, data.Type, location, direction,
            data.State, data.Data, data.UsagePolicy, data.OwnerId, data.OwnerName,
            data.SecondsToExpiration);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::roundLocation
    private IVector3d RoundLocation(IVector3d location)
    {
        if (_roomCreator == null)
        {
            return location;
        }

        RoomGeometry? geometry = _roomCreator.GetRoomGeometry(_currentRoomId);
        if (geometry == null)
        {
            return location;
        }

        // Snap Z coordinate to nearest pixel grid per AS3 logic
        // Uses screen projection to avoid sub-pixel rendering
        // TODO: Implement screen-projection rounding when RoomGeometry.GetScreenPosition is available
        return location;
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::setUserMovePosture
    private void SetUserMovePosture(int userId, string moveType)
    {
        if (_roomCreator == null)
        {
            return;
        }

        IRoomObject? obj = _roomCreator.GetRoom(_currentRoomId)?
            .GetObject(userId, RoomObjectCategoryEnum.OBJECT_CATEGORY_USER);
        if (obj == null)
        {
            return;
        }

        // Skip monsterplants
        string? objectType = obj.Type;
        if (objectType == RoomObjectUserTypes.MONSTERPLANT)
        {
            return;
        }

        string posture;
        switch (moveType)
        {
            case "mv":
                posture = "mv";
                break;
            case "sld":
            {
                // During slide, keep current posture unless it's "mv" (revert to "std")
                string? currentPosture = obj.Model?.GetString("figure_posture");
                posture = currentPosture == "mv" ? "std" : (currentPosture ?? "std");
                break;
            }
            default:
                posture = "std";
                break;
        }

        _roomCreator.UpdateObjectUserPosture(_currentRoomId, userId, posture);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::updateGuideMarker
    private void UpdateGuideMarker()
    {
        // TODO: Requires sessionDataManager.userId and userDataManager.getUserDataByType — unported
        // int ownUserId = _roomCreator.SessionDataManager.UserId;
        // SetUserGuideStatus(_guideUserId, _requesterUserId == ownUserId ? 1 : 0);
        // SetUserGuideStatus(_requesterUserId, _guideUserId == ownUserId ? 2 : 0);
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::removeGuideMarker
    private void RemoveGuideMarker()
    {
        // TODO: SetUserGuideStatus calls require session data manager — unported
        // SetUserGuideStatus(_guideUserId, 0);
        // SetUserGuideStatus(_requesterUserId, 0);
        _guideUserId = -1;
        _requesterUserId = -1;
    }

    /// @see com.sulake.habbo.room.RoomMessageHandler::setUserGuideStatus
    private void SetUserGuideStatus(int userId, int status)
    {
        // TODO: Requires roomSessionManager.GetSession().UserDataManager.GetUserDataByType(userId, 1) — unported
        // IUserData userData = _roomCreator.RoomSessionManager.GetSession(_currentRoomId)
        //     .UserDataManager.GetUserDataByType(userId, 1);
        // if (userData != null)
        //     _roomCreator.UpdateObjectUserAction(_currentRoomId, userData.RoomObjectId, "figure_guide_status", status);
    }

    #endregion
}
