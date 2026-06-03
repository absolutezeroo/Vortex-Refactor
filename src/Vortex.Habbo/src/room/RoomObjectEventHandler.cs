using System;
using System.Collections.Generic;
using System.Linq;

using Vortex.Core.Communication.Connection;
using Vortex.Core.Runtime.Events;
using Vortex.Room;
using Vortex.Room.Events;
using Vortex.Room.Object;
using Vortex.Room.Renderer;
using Vortex.Room.Utils;
using Vortex.Habbo.Communication.Messages.Outgoing.Room.Engine;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Habbo.Room.Object;
using Vortex.Habbo.Room.Utils;

namespace Vortex.Habbo.Room;

/// @see com.sulake.habbo.room.RoomObjectEventHandler
public class RoomObjectEventHandler : IRoomRenderingCanvasMouseListener, IDisposable
{
    private const string OBJECT_MOVE = "OBJECT_MOVE";
    private const string OBJECT_MOVE_TO = "OBJECT_MOVE_TO";
    private const string OBJECT_PLACE = "OBJECT_PLACE";
    private const string OBJECT_UNDEFINED = "";

    private const int CATEGORY_FLOOR = 10;
    private const int CATEGORY_WALL = 20;
    private const int CATEGORY_AVATAR = 100;
    private const int CATEGORY_SPECIAL = 200;

    private Dictionary<int, Dictionary<string, string>>? _mouseEventIdCache;
    private int _selectedAvatarId = -1;
    private int _previousSelectedObjectId = -1;
    private int _previousSelectedObjectCategory = -2;
    private string? _insertionMode;
    private RoomObjectMouseEvent? _lastMoveEvent;
    private IRoomEngineServices? _roomEngine;

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::RoomObjectEventHandler
    public RoomObjectEventHandler(IRoomEngineServices roomEngine)
    {
        _roomEngine = roomEngine;
        _mouseEventIdCache = new Dictionary<int, Dictionary<string, string>>();
    }

    protected IRoomEngineServices? GetRoomEngine()
    {
        return _roomEngine;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::dispose
    public void Dispose()
    {
        _mouseEventIdCache?.Clear();
        _mouseEventIdCache = null;
        _roomEngine = null;
        _lastMoveEvent = null;
    }

    #region Object Selection
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::getSelectedAvatarId
    public int GetSelectedAvatarId()
    {
        return _selectedAvatarId;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::setSelectedObject
    public void SetSelectedObject(int roomId, int objectId, int objectCategory)
    {
        if (_roomEngine == null)
        {
            return;
        }

        DeselectObject(roomId);

        if (objectCategory == CATEGORY_AVATAR)
        {
            SetSelectedAvatar(roomId, objectId, true);
        }
        else
        {
            IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, objectCategory);

            if (obj is IRoomObjectController
                {
                    EventHandler: not null
                } objController)
            {
                objController.EventHandler.ProcessUpdateMessage(new RoomObjectSelectedMessage(true));
            }

            _previousSelectedObjectId = objectId;
            _previousSelectedObjectCategory = objectCategory;
        }

        if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                RoomEngineObjectEvent.SELECTED, roomId, objectId, objectCategory));
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::setSelectedAvatar
    public void SetSelectedAvatar(int roomId, int avatarId, bool selected)
    {
        if (_roomEngine == null)
        {
            return;
        }

        // Deselect previous avatar
        if (_selectedAvatarId >= 0 && _selectedAvatarId != avatarId)
        {
            IRoomObject? prevAvatar = _roomEngine.GetRoomObject(roomId, _selectedAvatarId, CATEGORY_AVATAR);

            if (prevAvatar is IRoomObjectController
                {
                    EventHandler: not null
                } prevController)
            {
                prevController.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarSelectedMessage(false));
            }
        }

        _selectedAvatarId = selected ? avatarId : -1;

        IRoomObject? avatar = _roomEngine.GetRoomObject(roomId, avatarId, CATEGORY_AVATAR);

        if (avatar is IRoomObjectController
            {
                EventHandler: not null
            } avatarController)
        {
            avatarController.EventHandler.ProcessUpdateMessage(new RoomObjectAvatarSelectedMessage(selected));
        }

        // Update selection arrow visibility
        IRoomObjectController? arrow = _roomEngine.GetSelectionArrow(roomId);

        if (arrow != null)
        {
            arrow.EventHandler?.ProcessUpdateMessage(
                new RoomObjectVisibilityUpdateMessage(selected
                    ? RoomObjectVisibilityUpdateMessage.ENABLED
                    : RoomObjectVisibilityUpdateMessage.DISABLED));

            if (selected && avatar != null)
            {
                arrow.SetLocation(avatar.Location);
            }
        }

        // Send look-to if selecting own avatar
        if (selected && _roomEngine.Connection != null)
        {
            // TODO: Send LookToMessageComposer when ported
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::deselectObject
    private void DeselectObject(int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        if (_previousSelectedObjectId >= 0 && _previousSelectedObjectCategory >= 0)
        {
            IRoomObject? prevObj = _roomEngine.GetRoomObject(
                roomId, _previousSelectedObjectId, _previousSelectedObjectCategory);

            if (prevObj is IRoomObjectController
                {
                    EventHandler: not null
                } prevObjController)
            {
                prevObjController.EventHandler.ProcessUpdateMessage(new RoomObjectSelectedMessage(false));
            }
        }

        _previousSelectedObjectId = -1;
        _previousSelectedObjectCategory = -2;
    }
    #endregion

    #region Object Insertion
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::initializeRoomObjectInsert
    public bool InitializeRoomObjectInsert(string? catalogType, int roomId, int canvasId,
        int typeId, double floorHeight, string? furniName = null, IStuffData? stuffData = null,
        int state = -1, int animFrame = -1, string? posture = null, string? instanceData = null)
    {
        if (_roomEngine == null)
        {
            return false;
        }

        _insertionMode = catalogType;

        IRoomObjectController? iconCursor = _roomEngine.GetTileCursor(roomId);

        if (iconCursor?.EventHandler != null)
        {
            iconCursor.EventHandler.ProcessUpdateMessage(
                new RoomObjectVisibilityUpdateMessage(RoomObjectVisibilityUpdateMessage.DISABLED));
        }

        int category = _roomEngine.GetRoomObjectCategory(furniName ?? "");

        SetSelectedObjectData(roomId, -1, category,
            new Vector3d(), new Vector3d(),
            OBJECT_PLACE, typeId, instanceData, stuffData, state, animFrame, posture);

        return true;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::cancelRoomObjectInsert
    public bool CancelRoomObjectInsert(int roomId)
    {
        if (_roomEngine == null)
        {
            return false;
        }

        ResetSelectedObjectData(roomId);
        return true;
    }
    #endregion

    #region Mouse Event Processing
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::processRoomCanvasMouseEvent
    public void ProcessRoomCanvasMouseEvent(RoomSpriteMouseEvent mouseEvent,
        IRoomObject? obj, IRoomGeometry geometry)
    {
        if (_roomEngine == null || mouseEvent == null)
        {
            return;
        }

        int roomId = _roomEngine.ActiveRoomId;

        if (obj is IRoomObjectController
            {
                EventHandler: not null
            } objController)
        {
            objController.EventHandler.MouseEvent(mouseEvent, geometry);
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectEvent
    public void HandleRoomObjectEvent(RoomObjectEvent roomEvent, int roomId)
    {
        if (_roomEngine == null || roomEvent == null)
        {
            return;
        }

        switch (roomEvent)
        {
            case RoomObjectMouseEvent mouseEvent:
                HandleRoomObjectMouseEvent(mouseEvent, roomId);
                break;
            case RoomObjectStateChangeEvent stateEvent:
                HandleObjectStateChange(stateEvent, roomId);
                break;
            case RoomObjectWidgetRequestEvent widgetEvent:
                HandleObjectWidgetRequestEvent(widgetEvent, roomId);
                break;
            case RoomObjectPlaySoundIdEvent soundEvent:
                HandleRoomObjectPlaySoundEvent(soundEvent, roomId);
                break;
            case RoomObjectSamplePlaybackEvent sampleEvent:
                HandleRoomObjectSamplePlaybackEvent(sampleEvent, roomId);
                break;
            case RoomObjectFurnitureActionEvent actionEvent:
                HandleObjectActionEvent(actionEvent, roomId);
                break;
            case RoomObjectMoveEvent moveEvent:
                HandleObjectMoveEvent(moveEvent, roomId);
                break;
            case RoomObjectHSLColorEnableEvent hslEvent:
                HandleRoomObjectHSLColorEnableEvent(hslEvent, roomId);
                break;
            case RoomObjectDimmerStateUpdateEvent dimmerEvent:
                HandleObjectDimmerStateEvent(dimmerEvent, roomId);
                break;
            case RoomObjectFloorHoleEvent holeEvent:
                HandleObjectFloorHoleEvent(holeEvent, roomId);
                break;
            case RoomObjectRoomAdEvent adEvent:
                HandleObjectRoomAdEvent(adEvent, roomId);
                break;
            case RoomObjectBadgeAssetEvent badgeEvent:
                HandleObjectGroupBadgeEvent(badgeEvent, roomId);
                break;
            case RoomObjectDataRequestEvent dataRequest:
                HandleRoomObjectDataRequestEvent(dataRequest, roomId);
                break;
        }
    }
    #endregion

    #region Mouse Event Handlers
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectMouseEvent
    private void HandleRoomObjectMouseEvent(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        string type = mouseEvent.Type;

        // Handle tile mouse events for area selection
        if (mouseEvent is RoomObjectTileMouseEvent tileMouseEvent)
        {
            // TODO: Pass to RoomAreaSelectionManager when ported
        }

        switch (type)
        {
            case RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK:
                HandleRoomObjectMouseClick(mouseEvent, roomId);
                break;
            case RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_MOVE:
                HandleRoomObjectMouseMove(mouseEvent, roomId);
                break;
            case RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_DOWN:
                HandleRoomObjectMouseDown(mouseEvent, roomId);
                break;
            case RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_ENTER:
                HandleRoomObjectMouseEnter(mouseEvent, roomId);
                break;
            case RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_LEAVE:
                HandleRoomObjectMouseLeave(mouseEvent, roomId);
                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectMouseClick
    private void HandleRoomObjectMouseClick(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = mouseEvent.ObjectId;
        string? objectType = mouseEvent.ObjectType;
        int category = _roomEngine.GetRoomObjectCategory(objectType ?? "");

        // Check for duplicate event ID
        string? cachedId = GetMouseEventId(category, mouseEvent.Type);
        if (cachedId != null && cachedId == mouseEvent.EventId)
        {
            return;
        }
        SetMouseEventId(category, mouseEvent.Type, mouseEvent.EventId);

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);
        string operation = selectedData?.Operation ?? OBJECT_UNDEFINED;

        switch (operation)
        {
            case OBJECT_MOVE:
            case OBJECT_MOVE_TO:
                HandleObjectMoveClick(mouseEvent, roomId, selectedData);
                return;
            case OBJECT_PLACE:
                HandleObjectPlaceClick(mouseEvent, roomId, selectedData);
                return;
        }

        // Normal click handling
        bool isTileEvent = mouseEvent is RoomObjectTileMouseEvent;
        bool isWallEvent = mouseEvent is RoomObjectWallMouseEvent;

        if (isTileEvent)
        {
            HandleClickOnTile(roomId, (RoomObjectTileMouseEvent)mouseEvent);
            return;
        }

        if (category == CATEGORY_AVATAR)
        {
            if (mouseEvent.AltKey)
            {
                // Alt-click on pet/bot: request pickup
                // TODO: Send remove pet/bot composer
            }
            else if (mouseEvent.ShiftKey)
            {
                // Shift-click on pet: rotate
            }
            else
            {
                SetSelectedObject(roomId, objectId, CATEGORY_AVATAR);
                HandleClickOnAvatar(objectId, mouseEvent);
            }
            return;
        }

        if (category == CATEGORY_FLOOR)
        {
            if (mouseEvent.ShiftKey)
            {
                // Shift-click: request rotate
                if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
                {
                    dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                        RoomEngineObjectEvent.REQUEST_ROTATE, roomId, objectId, category));
                }
                return;
            }

            if (mouseEvent.CtrlKey)
            {
                // Ctrl-click: request pickup
                if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
                {
                    dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                        RoomEngineObjectEvent.REQUEST_PICKUP, roomId, objectId, category));
                }
                return;
            }

            // Handle "where you click where you go" on furniture
            if (HandleMoveTargetFurni(roomId, mouseEvent))
            {
                return;
            }

            SetSelectedObject(roomId, objectId, category);
            ClickRoomObject(mouseEvent);
            return;
        }

        if (category == CATEGORY_WALL)
        {
            if (mouseEvent.CtrlKey)
            {
                if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
                {
                    dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                        RoomEngineObjectEvent.REQUEST_PICKUP, roomId, objectId, category));
                }
                return;
            }

            SetSelectedObject(roomId, objectId, category);
            ClickRoomObject(mouseEvent);
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectMouseMove
    private void HandleRoomObjectMouseMove(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        _lastMoveEvent = mouseEvent;

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);
        string operation = selectedData?.Operation ?? OBJECT_UNDEFINED;

        if (operation is OBJECT_MOVE or OBJECT_MOVE_TO)
        {
            HandleObjectMove(mouseEvent, roomId);
            return;
        }

        if (operation == OBJECT_PLACE)
        {
            HandleObjectPlace(mouseEvent, roomId);
            return;
        }

        // Update tile cursor
        if (mouseEvent is RoomObjectTileMouseEvent tileEvent)
        {
            RoomObjectTileCursorUpdateMessage? cursorMsg = HandleMouseOverTile(tileEvent, roomId);

            if (cursorMsg != null)
            {
                IRoomObjectController? tileCursor = _roomEngine.GetTileCursor(roomId);
                tileCursor?.EventHandler?.ProcessUpdateMessage(cursorMsg);
            }
        }
        else
        {
            int category = _roomEngine.GetRoomObjectCategory(mouseEvent.ObjectType ?? "");

            if (category == CATEGORY_FLOOR)
            {
                RoomObjectTileCursorUpdateMessage? cursorMsg = HandleMouseOverObject(category, roomId, mouseEvent);

                if (cursorMsg != null)
                {
                    IRoomObjectController? tileCursor = _roomEngine.GetTileCursor(roomId);
                    tileCursor?.EventHandler?.ProcessUpdateMessage(cursorMsg);
                }
            }
            else
            {
                IRoomObjectController? tileCursor = _roomEngine.GetTileCursor(roomId);
                tileCursor?.EventHandler?.ProcessUpdateMessage(
                    new RoomObjectTileCursorUpdateMessage(null, 0, false, ""));
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectMouseDown
    private void HandleRoomObjectMouseDown(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = mouseEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(mouseEvent.ObjectType ?? "");

        if (category is CATEGORY_FLOOR or CATEGORY_WALL)
        {
            if (mouseEvent.AltKey || DecorateModeMove(mouseEvent))
            {
                if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
                {
                    dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                        RoomEngineObjectEvent.REQUEST_MOVE, roomId, objectId, category));
                }
            }
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectMouseEnter
    private void HandleRoomObjectMouseEnter(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = mouseEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(mouseEvent.ObjectType ?? "");

        if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                RoomEngineObjectEvent.MOUSE_ENTER, roomId, objectId, category));
        }

        if (category == CATEGORY_AVATAR && _roomEngine.IsGameMode)
        {
            HandleMouseEnterAvatar(objectId, mouseEvent, roomId);
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectMouseLeave
    private void HandleRoomObjectMouseLeave(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = mouseEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(mouseEvent.ObjectType ?? "");

        if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineObjectEvent(
                RoomEngineObjectEvent.MOUSE_LEAVE, roomId, objectId, category));
        }

        if (_roomEngine.IsGameMode)
        {
            IRoomObjectController? tileCursor = _roomEngine.GetTileCursor(roomId);
            tileCursor?.EventHandler?.ProcessUpdateMessage(
                new RoomObjectDataUpdateMessage(0, null));
        }
    }
    #endregion

    #region Click Handlers
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleClickOnTile
    private void HandleClickOnTile(int roomId, RoomObjectTileMouseEvent tileEvent)
    {
        if (_roomEngine == null)
        {
            return;
        }

        if (_roomEngine.IsGameMode)
        {
            // TODO: Delegate to gameEngine.handleClickOnTile when ported
            return;
        }

        if (_roomEngine.IsDecorateMode)
        {
            return;
        }

        // TODO: Check spectator mode via roomSessionManager

        WalkTo(tileEvent.TileXAsInt, tileEvent.TileYAsInt);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleClickOnAvatar
    private void HandleClickOnAvatar(int avatarId, RoomObjectMouseEvent mouseEvent)
    {
        if (_roomEngine == null)
        {
            return;
        }

        if (_roomEngine.IsGameMode)
        {
            // TODO: Delegate to gameEngine.handleClickOnHuman when ported
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleMouseEnterAvatar
    private void HandleMouseEnterAvatar(int avatarId, RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        // TODO: Update tile cursor with avatar position in game mode
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleMoveTargetFurni
    private bool HandleMoveTargetFurni(int roomId, RoomObjectMouseEvent mouseEvent)
    {
        if (_roomEngine == null || !_roomEngine.IsWhereYouClickWhereYouGo())
        {
            return false;
        }

        int objectId = mouseEvent.ObjectId;
        IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, CATEGORY_FLOOR);

        if (obj == null)
        {
            return false;
        }

        IVector3d? activeSurface = GetActiveSurfaceLocation(obj, mouseEvent);

        if (activeSurface != null)
        {
            WalkTo((int)activeSurface.X, (int)activeSurface.Y);
            return true;
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::getActiveSurfaceLocation
    private IVector3d? GetActiveSurfaceLocation(IRoomObject obj, RoomObjectMouseEvent mouseEvent)
    {
        if (obj.Model == null)
        {
            return null;
        }

        double canStandOn = obj.Model.GetNumber("furniture_can_stand_on");
        double canSitOn = obj.Model.GetNumber("furniture_can_sit_on");
        double canLayOn = obj.Model.GetNumber("furniture_can_lay_on");

        if (canStandOn <= 0 && canSitOn <= 0 && canLayOn <= 0)
        {
            return null;
        }

        double sizeX = obj.Model.GetNumber("furniture_size_x");
        double sizeY = obj.Model.GetNumber("furniture_size_y");
        double sizeZ = obj.Model.GetNumber("furniture_size_z");

        if (sizeX <= 0 || sizeY <= 0)
        {
            return null;
        }

        IVector3d loc = obj.Location;
        int tileX = (int)(loc.X + mouseEvent.LocalX / 64.0);
        int tileY = (int)(loc.Y + mouseEvent.LocalY / 64.0);

        if (tileX < loc.X || tileX >= loc.X + sizeX ||
            tileY < loc.Y || tileY >= loc.Y + sizeY)
        {
            return null;
        }

        return new Vector3d(tileX, tileY, loc.Z + sizeZ);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::walkTo
    private void WalkTo(int tileX, int tileY)
    {
        if (_roomEngine?.Connection == null || _roomEngine.IsMoveBlocked())
        {
            return;
        }

        _roomEngine.Connection.Send(new MoveAvatarMessageComposer(tileX, tileY));
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::clickRoomObject
    private void ClickRoomObject(RoomObjectMouseEvent mouseEvent)
    {
        if (_roomEngine?.Connection == null)
        {
            return;
        }

        if (mouseEvent.AltKey || mouseEvent.CtrlKey || mouseEvent.ShiftKey)
        {
            return;
        }

        int objectId = mouseEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(mouseEvent.ObjectType ?? "");

        switch (category)
        {
            case CATEGORY_FLOOR:
                _roomEngine.Connection.Send(new ClickFurniMessageComposer(objectId));
                break;
            case CATEGORY_WALL:
                _roomEngine.Connection.Send(new ClickFurniMessageComposer(-objectId));
                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::decorateModeMove
    private bool DecorateModeMove(RoomObjectMouseEvent mouseEvent)
    {
        return _roomEngine is
               {
                   IsDecorateMode: true,
               } &&
               mouseEvent is
               {
                   CtrlKey: false,
                   ShiftKey: false,
               };
    }
    #endregion

    #region Object Move/Place
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectMoveClick
    private void HandleObjectMoveClick(RoomObjectMouseEvent mouseEvent, int roomId,
        ISelectedRoomObjectData? selectedData)
    {
        if (_roomEngine == null || selectedData == null)
        {
            return;
        }

        int objectId = selectedData.Id;
        int category = selectedData.Category;

        // Complete the move
        ModifyRoomObject(roomId, objectId, category, OBJECT_MOVE_TO);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectPlaceClick
    private void HandleObjectPlaceClick(RoomObjectMouseEvent mouseEvent, int roomId,
        ISelectedRoomObjectData? selectedData)
    {
        if (_roomEngine == null || selectedData == null)
        {
            return;
        }

        bool isTileEvent = mouseEvent is RoomObjectTileMouseEvent;
        bool isWallEvent = mouseEvent is RoomObjectWallMouseEvent;

        PlaceObject(roomId, isTileEvent, isWallEvent);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectMove
    private void HandleObjectMove(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData == null)
        {
            return;
        }

        int objectId = selectedData.Id;
        int category = selectedData.Category;

        IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, category);

        if (obj is not IRoomObjectController controller)
        {
            return;
        }

        switch (mouseEvent)
        {
            case RoomObjectTileMouseEvent tileEvent:
                {
                    if (category is CATEGORY_FLOOR or CATEGORY_AVATAR)
                    {
                        FurniStackingHeightMap? heightMap = _roomEngine.GetFurniStackingHeightMap(roomId);

                        HandleFurnitureMove(controller, selectedData, tileEvent.TileXAsInt, tileEvent.TileYAsInt, heightMap);
                    }

                    break;
                }
            case RoomObjectWallMouseEvent wallEvent:
                {
                    if (category == CATEGORY_WALL)
                    {
                        HandleWallItemMove(controller, selectedData,
                            wallEvent.WallLocation, wallEvent.WallWidth, wallEvent.WallHeight,
                            wallEvent.X, wallEvent.Y, wallEvent.Direction);
                    }

                    break;
                }
        }

        // Semi-transparent during move
        SetObjectAlphaMultiplier(controller, 0.5);
        _roomEngine.SetObjectMoverIconSpriteVisible(false);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectPlace
    private void HandleObjectPlace(RoomObjectMouseEvent mouseEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData == null)
        {
            return;
        }

        int objectId = selectedData.Id;
        int category = selectedData.Category;

        // Create object if not placed yet
        if (objectId < 0)
        {
            // TODO: Create placement object
        }

        IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, category);

        if (obj is not IRoomObjectController controller)
        {
            return;
        }

        switch (mouseEvent)
        {
            case RoomObjectTileMouseEvent tileEvent:
                {
                    if (category is CATEGORY_FLOOR or CATEGORY_AVATAR)
                    {
                        FurniStackingHeightMap? heightMap = _roomEngine.GetFurniStackingHeightMap(roomId);

                        HandleFurnitureMove(controller, selectedData, tileEvent.TileXAsInt, tileEvent.TileYAsInt, heightMap);
                    }

                    break;
                }
            case RoomObjectWallMouseEvent wallEvent:
                {
                    if (category == CATEGORY_WALL)
                    {
                        HandleWallItemMove(controller, selectedData,
                            wallEvent.WallLocation, wallEvent.WallWidth, wallEvent.WallHeight,
                            wallEvent.X, wallEvent.Y, wallEvent.Direction);
                    }

                    break;
                }
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::placeObject
    private void PlaceObject(int roomId, bool isTileEvent, bool isWallEvent)
    {
        if (_roomEngine == null)
        {
            return;
        }

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData == null)
        {
            return;
        }

        int category = selectedData.Category;
        int objectId = selectedData.Id;

        if (_roomEngine.Connection != null)
        {
            IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, category);

            if (obj != null)
            {
                IVector3d loc = obj.Location;
                IVector3d dir = obj.Direction;
                int dirValue = (int)(dir.X / 45);

                if (category is CATEGORY_FLOOR or CATEGORY_WALL)
                {
                    string wallLocation = "";

                    if (category == CATEGORY_WALL)
                    {
                        LegacyWallGeometry? geometry = _roomEngine.GetLegacyGeometry(roomId);
                        wallLocation = geometry?.GetOldLocationString(loc, dir.X) ?? "";
                    }

                    _roomEngine.Connection.Send(new PlaceObjectMessageComposer(
                        objectId, category, wallLocation, (int)loc.X, (int)loc.Y, dirValue));
                }
                // TODO: Pets → PlacePetMessageComposer (not yet ported)
                // TODO: Bots → PlaceBotMessageComposer (not yet ported)
                // TODO: Stickie → PlacePostItMessageComposer (not yet ported)
            }
        }

        if (_roomEngine.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineObjectPlacedEvent(
                RoomEngineObjectEvent.PLACED, roomId, selectedData.Id, category,
                "", 0, 0, 0, 0, true,
                category == CATEGORY_FLOOR, category == CATEGORY_WALL,
                selectedData.InstanceData));
        }

        ResetSelectedObjectData(roomId);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::placeObjectOnUser
    private void PlaceObjectOnUser(int roomId, int objectId, int category,
        int droppedObjectId, int droppedObjectCategory)
    {
        if (_roomEngine?.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineObjectPlacedOnUserEvent(
                RoomEngineObjectEvent.PLACED_ON_USER, roomId, objectId, category,
                droppedObjectId, droppedObjectCategory));
        }
    }
    #endregion

    #region Furniture/Wall Item Validation
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleFurnitureMove
    private bool HandleFurnitureMove(IRoomObjectController controller,
        ISelectedRoomObjectData selectedData, int tileX, int tileY,
        FurniStackingHeightMap? heightMap)
    {
        if (controller.Model == null)
        {
            return false;
        }

        IVector3d savedDir = controller.Direction;
        IVector3d? targetLoc = ValidateFurnitureLocation(controller,
            new Vector3d(tileX, tileY, 0), controller.Location, controller.Direction, heightMap);

        if (targetLoc != null)
        {
            controller.SetLocation(targetLoc);
            return true;
        }

        // Try rotating
        List<int>? allowedDirs = controller.Model.GetNumberArray("furniture_allowed_directions");

        if (allowedDirs is
            {
                Count: > 0,
            })
        {
            foreach (Vector3d testDir in allowedDirs.Select(dir => new Vector3d(dir)))
            {
                controller.SetDirection(testDir);

                targetLoc = ValidateFurnitureLocation(controller,
                    new Vector3d(tileX, tileY, 0), controller.Location, testDir, heightMap);

                if (targetLoc == null)
                {
                    continue;
                }

                controller.SetLocation(targetLoc);

                return true;
            }
        }

        // Restore original direction
        controller.SetDirection(savedDir);
        return false;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleWallItemMove
    private bool HandleWallItemMove(IRoomObjectController controller,
        ISelectedRoomObjectData selectedData,
        IVector3d wallLocation, IVector3d wallWidth, IVector3d wallHeight,
        double x, double y, double direction)
    {
        IVector3d? targetLoc = ValidateWallItemLocation(controller,
            wallLocation, wallWidth, wallHeight, x, y, selectedData);

        if (targetLoc == null)
        {
            return false;
        }

        controller.SetLocation(targetLoc);
        controller.SetDirection(new Vector3d(direction));

        return true;

    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleUserPlace
    private bool HandleUserPlace(IRoomObjectController controller, int tileX, int tileY,
        LegacyWallGeometry? geometry)
    {
        if (geometry == null)
        {
            return false;
        }

        // TODO: Validate tile is walkable
        controller.SetLocation(new Vector3d(tileX, tileY, 0));
        return true;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::validateFurnitureLocation
    private IVector3d? ValidateFurnitureLocation(IRoomObject obj,
        IVector3d tilePos, IVector3d oldLoc, IVector3d oldDir,
        FurniStackingHeightMap? heightMap)
    {
        if (obj.Model == null || heightMap == null)
        {
            return null;
        }

        int sizeX = (int)obj.Model.GetNumber("furniture_size_x");
        int sizeY = (int)obj.Model.GetNumber("furniture_size_y");
        double sizeZ = obj.Model.GetNumber("furniture_size_z");
        bool alwaysStackable = obj.Model.GetNumber("furniture_always_stackable") > 0;

        // Handle rotation: swap sizeX/sizeY for 90/270 degree rotations
        int dir = (int)(obj.Direction.X / 45) % 8;

        if (dir is 1 or 3 or 5 or 7)
        {
            (sizeX, sizeY) = (sizeY, sizeX);
        }

        int targetX = (int)tilePos.X;
        int targetY = (int)tilePos.Y;
        int oldX = (int)oldLoc.X;
        int oldY = (int)oldLoc.Y;

        bool valid = heightMap.ValidateLocation(
            targetX, targetY, sizeX, sizeY,
            oldX, oldY, sizeX, sizeY,
            alwaysStackable);

        if (!valid)
        {
            return null;
        }

        double tileHeight = heightMap.GetTileHeight(targetX, targetY);
        return new Vector3d(targetX, targetY, tileHeight);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::validateWallItemLocation
    private IVector3d? ValidateWallItemLocation(IRoomObject obj,
        IVector3d wallLocation, IVector3d wallWidth, IVector3d wallHeight,
        double x, double y, ISelectedRoomObjectData selectedData)
    {
        if (obj.Model == null)
        {
            return null;
        }

        double sizeX = obj.Model.GetNumber("furniture_size_x");
        double sizeZ = obj.Model.GetNumber("furniture_size_z");
        double centerZ = obj.Model.GetNumber("furniture_center_z");

        double wallLen = Math.Sqrt(
            wallWidth.X * wallWidth.X + wallWidth.Y * wallWidth.Y + wallWidth.Z * wallWidth.Z);
        double wallHLen = Math.Sqrt(
            wallHeight.X * wallHeight.X + wallHeight.Y * wallHeight.Y + wallHeight.Z * wallHeight.Z);

        if (wallLen == 0 || wallHLen == 0)
        {
            return null;
        }

        // Clamp to wall bounds
        double clampedX = Math.Max(sizeX / 2.0, Math.Min(x, wallLen - sizeX / 2.0));
        double clampedY = Math.Max(centerZ, Math.Min(y, wallHLen - (sizeZ - centerZ)));

        return new Vector3d(
            wallLocation.X + wallWidth.X * clampedX / wallLen + wallHeight.X * clampedY / wallHLen,
            wallLocation.Y + wallWidth.Y * clampedX / wallLen + wallHeight.Y * clampedY / wallHLen,
            wallLocation.Z + wallWidth.Z * clampedX / wallLen + wallHeight.Z * clampedY / wallHLen);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::getValidRoomObjectDirection
    public int GetValidRoomObjectDirection(IRoomObjectController controller, bool rotatePositive)
    {
        if (controller.Model == null)
        {
            return 0;
        }

        List<int>? allowedDirs = controller.Model.GetNumberArray("furniture_allowed_directions");

        if (allowedDirs == null || allowedDirs.Count == 0)
        {
            return 0;
        }

        int currentDir = (int)controller.Direction.X;
        int currentIndex = allowedDirs.IndexOf(currentDir);

        if (currentIndex < 0)
        {
            return allowedDirs[0];
        }

        if (rotatePositive)
        {
            return allowedDirs[(currentIndex + 1) % allowedDirs.Count];
        }
        else
        {
            return allowedDirs[(currentIndex - 1 + allowedDirs.Count) % allowedDirs.Count];
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::validateFurnitureDirection
    private bool ValidateFurnitureDirection(IRoomObjectController controller,
        IVector3d location, IVector3d newDirection, FurniStackingHeightMap? heightMap)
    {
        IVector3d savedDir = controller.Direction;
        controller.SetDirection(newDirection);

        IVector3d? result = ValidateFurnitureLocation(controller, location, location, newDirection, heightMap);

        controller.SetDirection(savedDir);
        return result != null;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::setObjectAlphaMultiplier
    private void SetObjectAlphaMultiplier(IRoomObjectController controller, double alpha)
    {
        controller.ModelController?.SetNumber("furniture_alpha_multiplier", alpha);
    }
    #endregion

    #region Object Modification
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::modifyRoomObject
    public bool ModifyRoomObject(int roomId, int objectId, int category, string operation)
    {
        if (_roomEngine == null)
        {
            return false;
        }

        IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, category);

        if (obj is not IRoomObjectController controller)
        {
            return false;
        }

        switch (operation)
        {
            case "ROTATE_POSITIVE":
            case "ROTATE_NEGATIVE":
                {
                    int newDir = GetValidRoomObjectDirection(controller, operation == "ROTATE_POSITIVE");
                    IVector3d loc = controller.Location;

                    if (_roomEngine.Connection == null)
                    {
                        return true;
                    }

                    switch (category)
                    {
                        case CATEGORY_FLOOR:
                            {
                                FurniStackingHeightMap? heightMap = _roomEngine.GetFurniStackingHeightMap(roomId);

                                if (ValidateFurnitureDirection(controller, loc, new Vector3d(newDir), heightMap))
                                {
                                    _roomEngine.Connection.Send(new MoveObjectMessageComposer(
                                        objectId, (int)loc.X, (int)loc.Y, newDir / 45));
                                }

                                break;
                            }
                        case CATEGORY_WALL:
                            {
                                LegacyWallGeometry? geometry = _roomEngine.GetLegacyGeometry(roomId);
                                string? locationStr = geometry?.GetOldLocationString(loc, controller.Direction.X);

                                if (locationStr != null)
                                {
                                    _roomEngine.Connection.Send(new MoveWallItemMessageComposer(objectId, locationStr));
                                }

                                break;
                            }
                    }

                    return true;
                }
            case "PICKUP":
            case "EJECT":
                {
                    _roomEngine.Connection?.Send(new PickupObjectMessageComposer(objectId, category));

                    return true;
                }
            case "MOVE":
                {
                    SetSelectedObjectData(roomId, objectId, category,
                        controller.Location, controller.Direction, OBJECT_MOVE);

                    return true;
                }
            case OBJECT_MOVE_TO:
                {
                    IVector3d loc = controller.Location;
                    IVector3d dir = controller.Direction;

                    if (_roomEngine.Connection != null)
                    {
                        switch (category)
                        {
                            case CATEGORY_FLOOR:
                                {
                                    int dirValue = (int)(dir.X / 45);
                                    _roomEngine.Connection.Send(new MoveObjectMessageComposer(
                                        objectId, (int)loc.X, (int)loc.Y, dirValue));

                                    break;
                                }
                            case CATEGORY_WALL:
                                {
                                    LegacyWallGeometry? geometry = _roomEngine.GetLegacyGeometry(roomId);
                                    string? locationStr = geometry?.GetOldLocationString(loc, dir.X);

                                    if (locationStr != null)
                                    {
                                        _roomEngine.Connection.Send(new MoveWallItemMessageComposer(objectId, locationStr));
                                    }

                                    break;
                                }
                        }
                    }

                    SetObjectAlphaMultiplier(controller, 1.0);
                    ResetSelectedObjectData(roomId);

                    return true;
                }
            case "PICKUP_PET":
            case "PICKUP_BOT":
                {
                    // TODO: Send dedicated PlacePetMessageComposer/PlaceBotMessageComposer when ported
                    _roomEngine.Connection?.Send(new PickupObjectMessageComposer(objectId, category));

                    return true;
                }
        }

        return false;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::modifyRoomObjectData
    public bool ModifyRoomObjectData(int roomId, int objectId, int category,
        string operation, Dictionary<string, string>? dataMap)
    {
        if (_roomEngine?.Connection == null)
        {
            return false;
        }

        if (operation != "OBJECT_SAVE_STUFF_DATA" || dataMap == null)
        {
            return false;
        }

        _roomEngine.Connection.Send(new SetObjectDataMessageComposer(objectId, dataMap));

        return true;

    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::modifyWallItemData
    public bool ModifyWallItemData(int roomId, int objectId, string variable, string value)
    {
        if (_roomEngine?.Connection == null)
        {
            return false;
        }

        _roomEngine.Connection.Send(new SetItemDataMessageComposer(objectId, variable, value));

        return true;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::deleteWallItem
    public bool DeleteWallItem(int roomId, int objectId)
    {
        if (_roomEngine?.Connection == null)
        {
            return false;
        }

        _roomEngine.Connection.Send(new RemoveItemMessageComposer(objectId));

        return true;
    }
    #endregion

    #region Recalibration
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::recalibrateMovements
    public void RecalibrateMovements(int roomId, bool validateMode = false)
    {
        if (_lastMoveEvent != null)
        {
            HandleRoomObjectMouseMove(_lastMoveEvent, roomId);
        }
    }
    #endregion

    #region Event-Specific Handlers
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectStateChange
    private void HandleObjectStateChange(RoomObjectStateChangeEvent stateEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        bool isRandom = stateEvent.Type == RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_RANDOM;
        int objectId = stateEvent.ObjectId;
        string? objectType = stateEvent.ObjectType;
        int category = _roomEngine.GetRoomObjectCategory(objectType ?? "");

        ChangeRoomObjectState(roomId, objectId, category, stateEvent.Param, isRandom);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::changeRoomObjectState
    private bool ChangeRoomObjectState(int roomId, int objectId, int category,
        int state, bool isRandom)
    {
        if (_roomEngine?.Connection == null)
        {
            return false;
        }

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData?.Operation == OBJECT_PLACE)
        {
            return false;
        }

        switch (category)
        {
            case CATEGORY_FLOOR:
                {
                    if (!isRandom)
                    {
                        _roomEngine.Connection.Send(new UseFurnitureMessageComposer(objectId, state));
                    }
                    // else: SetRandomStateMessageComposer — not yet ported

                    break;
                }
            case CATEGORY_WALL:
                _roomEngine.Connection.Send(new UseWallItemMessageComposer(objectId, state));

                break;
        }

        return true;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectActionEvent (useObject)
    private void HandleObjectActionEvent(RoomObjectFurnitureActionEvent actionEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        string type = actionEvent.Type;

        switch (type)
        {
            case RoomObjectFurnitureActionEvent.DICE_ACTIVATE:
            case RoomObjectFurnitureActionEvent.DICE_OFF:
            case RoomObjectFurnitureActionEvent.USE_HABBOWHEEL:
            case RoomObjectFurnitureActionEvent.STICKIE:
            case RoomObjectFurnitureActionEvent.ENTER_ONEWAYDOOR:
                UseObject(roomId, actionEvent.ObjectId, actionEvent.ObjectType, type);

                break;
            case RoomObjectFurnitureActionEvent.SOUND_MACHINE_INIT:
            case RoomObjectFurnitureActionEvent.SOUND_MACHINE_START:
            case RoomObjectFurnitureActionEvent.SOUND_MACHINE_STOP:
            case RoomObjectFurnitureActionEvent.SOUND_MACHINE_DISPOSE:
                HandleObjectSoundMachineEvent(actionEvent, roomId);

                break;
            case RoomObjectFurnitureActionEvent.JUKEBOX_INIT:
            case RoomObjectFurnitureActionEvent.JUKEBOX_START:
            case RoomObjectFurnitureActionEvent.JUKEBOX_MACHINE_STOP:
            case RoomObjectFurnitureActionEvent.JUKEBOX_DISPOSE:
                HandleObjectJukeboxEvent(actionEvent, roomId);

                break;
            case RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW:
            case RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON:
                HandleRoomActionMouseRequestEvent(actionEvent, roomId);

                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::useObject
    private void UseObject(int roomId, int objectId, string? objectType, string eventType)
    {
        if (_roomEngine?.Connection == null)
        {
            return;
        }

        int category = _roomEngine.GetRoomObjectCategory(objectType ?? "");

        switch (eventType)
        {
            case RoomObjectFurnitureActionEvent.STICKIE:
                _roomEngine.Connection.Send(new GetItemDataMessageComposer(objectId));
                break;
            // TODO: DICE_ACTIVATE → ThrowDiceMessageComposer (not yet ported)
            // TODO: DICE_OFF → DiceOffMessageComposer (not yet ported)
            // TODO: USE_HABBOWHEEL → SpinWheelOfFortuneMessageComposer (not yet ported)
            // TODO: ENTER_ONEWAYDOOR → EnterOneWayDoorMessageComposer (not yet ported)
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectWidgetRequestEvent
    private void HandleObjectWidgetRequestEvent(RoomObjectWidgetRequestEvent widgetEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = widgetEvent.ObjectId;
        string? objectType = widgetEvent.ObjectType;
        int category = _roomEngine.GetRoomObjectCategory(objectType ?? "");

        if (_roomEngine.Events is not EventDispatcherWrapper dispatcher)
        {
            return;
        }

        string type = widgetEvent.Type;

        switch (type)
        {
            case RoomObjectWidgetRequestEvent.OPEN_WIDGET:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_OPEN_WIDGET, roomId, objectId, category,
                    widgetEvent.Widget));

                break;
            case RoomObjectWidgetRequestEvent.CLOSE_WIDGET:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_CLOSE_WIDGET, roomId, objectId, category,
                    widgetEvent.Widget));

                break;
            case RoomObjectWidgetRequestEvent.OPEN_FURNI_CONTEXT_MENU:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_OPEN_FURNI_CONTEXT_MENU, roomId, objectId, category,
                    widgetEvent.ContextMenu));

                break;
            case RoomObjectWidgetRequestEvent.CLOSE_FURNI_CONTEXT_MENU:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_CLOSE_FURNI_CONTEXT_MENU, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.PLACEHOLDER:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_PLACEHOLDER, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.CREDITFURNI:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_CREDITFURNI, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.STICKIE:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_STICKIE, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.PRESENT:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_PRESENT, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.TROPHY:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_TROPHY, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.TEASER:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_TEASER, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.ECOTRONBOX:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_ECOTRONBOX, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.DIMMER:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_DIMMER, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.WIDGET_REMOVE_DIMMER:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REMOVE_DIMMER, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.CLOTHING_CHANGE:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_CLOTHING_CHANGE, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.JUKEBOX_PLAYLIST_EDITOR:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_PLAYLIST_EDITOR, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.MANNEQUIN:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_MANNEQUIN, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_MONSTERPLANT_SEED_PLANT_CONFIRMATION_DIALOG, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.PURCHASABLE_CLOTHING_CONFIRMATION_DIALOG:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_PURCHASABLE_CLOTHING_CONFIRMATION_DIALOG, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.BACKGROUND_COLOR:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_BACKGROUND_COLOR, roomId, objectId, category));
 break;
            case RoomObjectWidgetRequestEvent.MYSTERYBOX_OPEN_DIALOG:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_MYSTERYBOX_OPEN_DIALOG, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.EFFECTBOX_OPEN_DIALOG:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_EFFECTBOX_OPEN_DIALOG, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.MYSTERYTROPHY_OPEN_DIALOG:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_MYSTERYTROPHY_OPEN_DIALOG, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_OPEN:
                // TODO: Send GetResolutionAchievementsMessageComposer when ported
                break;
            case RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_ENGRAVING:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_ACHIEVEMENT_RESOLUTION_ENGRAVING, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_FAILED:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_ACHIEVEMENT_RESOLUTION_FAILED, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.FRIEND_FURNITURE_CONFIRM:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_FRIEND_FURNITURE_CONFIRM, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.FRIEND_FURNITURE_ENGRAVING:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_FRIEND_FURNITURE_ENGRAVING, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.BADGE_DISPLAY_ENGRAVING:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_BADGE_DISPLAY_ENGRAVING, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.HIGH_SCORE_DISPLAY:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_HIGH_SCORE_DISPLAY, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.HIDE_HIGH_SCORE_DISPLAY:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_HIDE_HIGH_SCORE_DISPLAY, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.INTERNAL_LINK:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_INTERNAL_LINK, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.ROOM_LINK:
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_ROOM_LINK, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.PET_PRODUCT_MENU:
                dispatcher.DispatchEvent(new RoomEngineUseProductEvent(
                    RoomEngineUseProductEvent.USE_PRODUCT_FROM_ROOM, roomId, objectId, category));

                break;
            case RoomObjectWidgetRequestEvent.GUILD_FURNI_CONTEXT_MENU:
                // TODO: Send GetGuildFurniContextMenuInfoMessageComposer when ported
                dispatcher.DispatchEvent(new RoomEngineToWidgetEvent(
                    RoomEngineToWidgetEvent.REQUEST_OPEN_FURNI_CONTEXT_MENU, roomId, objectId, category,
                    "guild_custom_furni"));

                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectMoveEvent
    private void HandleObjectMoveEvent(RoomObjectMoveEvent moveEvent, int roomId)
    {
        switch (moveEvent.Type)
        {
            case RoomObjectMoveEvent.POSITION_CHANGED:
                HandleSelectedObjectMove(moveEvent, roomId);

                break;
            case RoomObjectMoveEvent.OBJECT_REMOVED:
                HandleSelectedObjectRemove(moveEvent, roomId);

                break;
            case RoomObjectMoveEvent.SLIDE_ANIMATION:
                HandleObjectSlide(moveEvent, roomId);

                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleSelectedObjectMove
    private void HandleSelectedObjectMove(RoomObjectEvent roomEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = roomEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(roomEvent.ObjectType ?? "");

        if (objectId != _selectedAvatarId || category != CATEGORY_AVATAR)
        {
            return;
        }

        IRoomObjectController? arrow = _roomEngine.GetSelectionArrow(roomId);
        IRoomObject? avatar = _roomEngine.GetRoomObject(roomId, objectId, CATEGORY_AVATAR);

        if (arrow != null && avatar != null)
        {
            arrow.SetLocation(avatar.Location);
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleSelectedObjectRemove
    private void HandleSelectedObjectRemove(RoomObjectEvent roomEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = roomEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(roomEvent.ObjectType ?? "");

        if (objectId == _selectedAvatarId && category == CATEGORY_AVATAR)
        {
            _selectedAvatarId = -1;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectSlide
    private void HandleObjectSlide(RoomObjectEvent roomEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int category = _roomEngine.GetRoomObjectCategory(roomEvent.ObjectType ?? "");

        if (category == CATEGORY_WALL)
        {
            _roomEngine.UpdateObjectRoomWindow(roomId, roomEvent.ObjectId);
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectDimmerStateEvent
    private void HandleObjectDimmerStateEvent(RoomObjectDimmerStateUpdateEvent dimmerEvent, int roomId)
    {
        if (_roomEngine?.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineDimmerStateEvent(
                roomId, dimmerEvent.ObjectId,
                dimmerEvent.State, dimmerEvent.PresetId,
                dimmerEvent.EffectId, dimmerEvent.Color, (uint)dimmerEvent.Brightness));
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectSoundMachineEvent
    private void HandleObjectSoundMachineEvent(RoomObjectFurnitureActionEvent actionEvent, int roomId)
    {
        if (_roomEngine?.Events is not EventDispatcherWrapper dispatcher)
        {
            return;
        }

        // Skip if object is being placed
        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData?.Operation == OBJECT_PLACE)
        {
            return;
        }

        int objectId = actionEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(actionEvent.ObjectType ?? "");

        dispatcher.DispatchEvent(new RoomEngineSoundMachineEvent(
            actionEvent.Type, roomId, objectId, category));
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectJukeboxEvent
    private void HandleObjectJukeboxEvent(RoomObjectFurnitureActionEvent actionEvent, int roomId)
    {
        if (_roomEngine?.Events is not EventDispatcherWrapper dispatcher)
        {
            return;
        }

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData?.Operation == OBJECT_PLACE)
        {
            return;
        }

        int objectId = actionEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(actionEvent.ObjectType ?? "");

        dispatcher.DispatchEvent(new RoomEngineSoundMachineEvent(
            actionEvent.Type, roomId, objectId, category));
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectFloorHoleEvent
    private void HandleObjectFloorHoleEvent(RoomObjectFloorHoleEvent holeEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = holeEvent.ObjectId;

        switch (holeEvent.Type)
        {
            case RoomObjectFloorHoleEvent.ADD_HOLE:
                _roomEngine.AddFloorHole(roomId, objectId);

                break;
            case RoomObjectFloorHoleEvent.REMOVE_HOLE:
                _roomEngine.RemoveFloorHole(roomId, objectId);

                break;
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectRoomAdEvent
    private void HandleObjectRoomAdEvent(RoomObjectRoomAdEvent adEvent, int roomId)
    {
        if (_roomEngine?.Events is not EventDispatcherWrapper dispatcher)
        {
            return;
        }

        int objectId = adEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(adEvent.ObjectType ?? "");

        dispatcher.DispatchEvent(new RoomEngineRoomAdEvent(
            adEvent.Type, roomId, objectId, category));
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleObjectGroupBadgeEvent
    private void HandleObjectGroupBadgeEvent(RoomObjectBadgeAssetEvent badgeEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = badgeEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(badgeEvent.ObjectType ?? "");

        // TODO: Extract badge ID and call _roomEngine.RequestBadgeImageAsset
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomActionMouseRequestEvent
    private void HandleRoomActionMouseRequestEvent(RoomObjectFurnitureActionEvent actionEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        int objectId = actionEvent.ObjectId;
        string? objectType = actionEvent.ObjectType;

        _roomEngine.RequestMouseCursor(actionEvent.Type, objectId, objectType ?? "");
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectPlaySoundEvent
    private void HandleRoomObjectPlaySoundEvent(RoomObjectPlaySoundIdEvent soundEvent, int roomId)
    {
        if (_roomEngine?.Events is not EventDispatcherWrapper dispatcher)
        {
            return;
        }

        int objectId = soundEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(soundEvent.ObjectType ?? "");

        dispatcher.DispatchEvent(new RoomEngineObjectPlaySoundEvent(
            soundEvent.Type, roomId, objectId, category,
            soundEvent.SoundId, soundEvent.Pitch));
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectSamplePlaybackEvent
    private void HandleRoomObjectSamplePlaybackEvent(RoomObjectSamplePlaybackEvent sampleEvent, int roomId)
    {
        if (_roomEngine?.Events is not EventDispatcherWrapper dispatcher)
        {
            return;
        }

        int objectId = sampleEvent.ObjectId;
        int category = _roomEngine.GetRoomObjectCategory(sampleEvent.ObjectType ?? "");

        dispatcher.DispatchEvent(new RoomEngineObjectSamplePlaybackEvent(
            sampleEvent.Type, roomId, objectId, category,
            sampleEvent.SampleId, sampleEvent.Pitch));
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectHSLColorEnableEvent
    private void HandleRoomObjectHSLColorEnableEvent(RoomObjectHSLColorEnableEvent hslEvent, int roomId)
    {
        if (_roomEngine?.Events is EventDispatcherWrapper dispatcher)
        {
            dispatcher.DispatchEvent(new RoomEngineHSLColorEnableEvent(
                RoomEngineHSLColorEnableEvent.ROOM_BACKGROUND_COLOR, roomId,
                hslEvent.Enable, hslEvent.Hue, hslEvent.Saturation, hslEvent.Lightness));
        }
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleRoomObjectDataRequestEvent
    private void HandleRoomObjectDataRequestEvent(RoomObjectDataRequestEvent dataRequest, int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        IRoomObject? obj = dataRequest.Object;

        if (obj is not IRoomObjectController controller)
        {
            return;
        }

        string type = dataRequest.Type;

        switch (type)
        {
            case RoomObjectDataRequestEvent.CURRENT_USER_ID:
                // TODO: Set session_current_user_id on model when session manager is ported

                break;
            case RoomObjectDataRequestEvent.URL_PREFIX:
                {
                    string? urlPrefix = _roomEngine.Configuration?.PropertyExists("dynamic.download.url") == true
                        ? _roomEngine.Configuration.GetProperty("dynamic.download.url")
                        : null;

                    if (urlPrefix != null)
                    {
                        controller.ModelController?.SetString("session_url_prefix", urlPrefix);
                    }

                    break;
                }
        }
    }
    #endregion

    #region Tile Cursor Helpers
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleMouseOverTile
    private RoomObjectTileCursorUpdateMessage? HandleMouseOverTile(
        RoomObjectTileMouseEvent tileEvent, int roomId)
    {
        if (_roomEngine == null)
        {
            return null;
        }

        int tileX = tileEvent.TileXAsInt;
        int tileY = tileEvent.TileYAsInt;
        double tileZ = tileEvent.TileZ;

        TileObjectMap? tileMap = _roomEngine.GetTileObjectMap(roomId);

        if (tileMap == null)
        {
            return new RoomObjectTileCursorUpdateMessage(new Vector3d(tileX, tileY, tileZ), 0, true, "");
        }

        IRoomObject? tileObj = tileMap.GetObjectIntTile(tileX, tileY);

        if (tileObj == null)
        {
            return new RoomObjectTileCursorUpdateMessage(new Vector3d(tileX, tileY, tileZ), 0, true, "");
        }

        FurniStackingHeightMap? heightMap = _roomEngine.GetFurniStackingHeightMap(roomId);

        if (heightMap == null)
        {
            return new RoomObjectTileCursorUpdateMessage(new Vector3d(tileX, tileY, tileZ), 0, true, "");
        }

        double extraHeight = heightMap.GetTileHeight(tileX, tileY) - tileZ;

        if (extraHeight > 0)
        {
            tileZ += extraHeight;
        }

        return new RoomObjectTileCursorUpdateMessage(new Vector3d(tileX, tileY, tileZ), 0, true, "");
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::handleMouseOverObject
    private RoomObjectTileCursorUpdateMessage? HandleMouseOverObject(
        int category, int roomId, RoomObjectMouseEvent mouseEvent)
    {
        if (_roomEngine == null || category != CATEGORY_FLOOR)
        {
            return null;
        }

        int objectId = mouseEvent.ObjectId;
        IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, category);

        if (obj == null)
        {
            return null;
        }

        IVector3d? activeSurface = GetActiveSurfaceLocation(obj, mouseEvent);

        if (activeSurface != null)
        {
            return new RoomObjectTileCursorUpdateMessage(activeSurface, 0, true, "");
        }

        return null;
    }
    #endregion

    #region Selected Object Data Helpers
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::setSelectedObjectData
    private void SetSelectedObjectData(int roomId, int id, int category,
        IVector3d location, IVector3d direction, string operation,
        int typeId = 0, string? instanceData = null, IStuffData? stuffData = null,
        int state = -1, int animFrame = -1, string? posture = null)
    {
        if (_roomEngine == null)
        {
            return;
        }

        SelectedRoomObjectData data = new(id, category, operation,
            location, direction, typeId, instanceData, stuffData, state, animFrame, posture);

        _roomEngine.SetSelectedObjectData(roomId, data);
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::resetSelectedObjectData
    private void ResetSelectedObjectData(int roomId)
    {
        if (_roomEngine == null)
        {
            return;
        }

        ISelectedRoomObjectData? selectedData = _roomEngine.GetSelectedObjectData(roomId);

        if (selectedData == null)
        {
            return;
        }

        string operation = selectedData.Operation;
        int objectId = selectedData.Id;
        int category = selectedData.Category;

        switch (operation)
        {
            case OBJECT_MOVE or OBJECT_MOVE_TO:
                {
                    // Revert location and alpha
                    IRoomObject? obj = _roomEngine.GetRoomObject(roomId, objectId, category);

                    if (obj is IRoomObjectController controller)
                    {
                        if (selectedData.Loc != null)
                        {
                            controller.SetLocation(selectedData.Loc);
                        }

                        if (selectedData.Dir != null)
                        {
                            controller.SetDirection(selectedData.Dir);
                        }

                        SetObjectAlphaMultiplier(controller, 1.0);
                    }

                    break;
                }
            case OBJECT_PLACE:
                {
                    // Dispose the uncompleted placement object
                    if (objectId >= 0)
                    {
                        IRoomInstance? room = _roomEngine.GetRoom(roomId);
                        room?.DisposeObject(objectId, category);
                    }

                    break;
                }
        }

        _roomEngine.SetSelectedObjectData(roomId, null);
    }
    #endregion

    #region Mouse Event ID Cache
    /// @see com.sulake.habbo.room.RoomObjectEventHandler::setMouseEventId
    private void SetMouseEventId(int category, string eventType, string eventId)
    {
        if (_mouseEventIdCache == null)
        {
            return;
        }

        if (!_mouseEventIdCache.TryGetValue(category, out Dictionary<string, string>? categoryCache))
        {
            categoryCache = new Dictionary<string, string>(StringComparer.Ordinal);
            _mouseEventIdCache[category] = categoryCache;
        }

        categoryCache[eventType] = eventId;
    }

    /// @see com.sulake.habbo.room.RoomObjectEventHandler::getMouseEventId
    private string? GetMouseEventId(int category, string eventType)
    {
        if (_mouseEventIdCache == null)
        {
            return null;
        }

        if (!_mouseEventIdCache.TryGetValue(category, out Dictionary<string, string>? categoryCache))
        {
            return null;
        }

        categoryCache.TryGetValue(eventType, out string? eventId);

        return eventId;
    }
    #endregion
}
