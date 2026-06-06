using System;

using Godot;

using Vortex.Core.Runtime;
using Vortex.Core.Runtime.Events;
using Vortex.Room.Object;
using Vortex.Room.Utils;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Object;
using Vortex.Habbo.Room.Object.Data;
using Vortex.Room.Object.Visualization;

using Environment = System.Environment;

namespace Vortex.Habbo.Room.Preview;

/// <summary>
/// Furniture/avatar preview for catalog, profile, and widget use.
/// Creates a temporary preview room and manages object placement, auto-zoom,
/// and canvas offset centering.
/// </summary>
/// @see com.sulake.habbo.room.preview.RoomPreviewer
public class RoomPreviewer
{
    private const int PREVIEW_CANVAS_ID = 1;
    private const int PREVIEW_OBJECT_ID = 1;
    private const int PREVIEW_OBJECT_LOCATION_X = 2;
    private const int PREVIEW_OBJECT_LOCATION_Y = 2;
    private const double ALLOWED_IMAGE_CUT = 0.25;
    private const int AUTOMATIC_STATE_CHANGE_INTERVAL = 2500;

    public const int SCALE_NORMAL = 64;
    public const int SCALE_SMALL = 32;

    private readonly IRoomEngine _roomEngine;
    private int _currentPreviewObjectType;
    private int _currentPreviewObjectCategory;
    private string _currentPreviewObjectData = "";
    private Rect2? _currentPreviewRectangle;
    private int _currentPreviewCanvasWidth;
    private int _currentPreviewCanvasHeight;
    private int _currentPreviewScale = SCALE_NORMAL;
    private bool _currentPreviewNeedsZoomOut;
    private bool _automaticStateChange;
    private long _previousAutomaticStateChangeTimeMs;
    private Vector2 _addViewOffset = Vector2.Zero;
    private bool _disableUpdate;

    private readonly Action<object?> _onRoomObjectAdded;
    private readonly Action<object?> _onRoomInitialized;

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::RoomPreviewer
    public RoomPreviewer(IRoomEngine roomEngine, int id = 1)
    {
        _roomEngine = roomEngine;
        PreviewRoomId = RoomId.MakeRoomPreviewerId(id);

        _onRoomObjectAdded = OnRoomObjectAdded;
        _onRoomInitialized = OnRoomInitialized;

        if (IsRoomEngineReady)
        {
            EventDispatcherWrapper? events = GetEvents();
            events?.AddEventListener(RoomEngineObjectEvent.ADDED, _onRoomObjectAdded);
            events?.AddEventListener(RoomEngineObjectEvent.CONTENT_UPDATED, _onRoomObjectAdded);
            events?.AddEventListener(RoomEngineEvent.ROOM_INITIALIZED, _onRoomInitialized);
        }

        CreateRoomForPreviews();
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::get addViewOffset
    public Vector2 AddViewOffset
    {
        get => _addViewOffset;
        set => _addViewOffset = value;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::set disableUpdate
    public bool DisableUpdate
    {
        set => _disableUpdate = value;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::set disableRoomEngineUpdate
    public bool DisableRoomEngineUpdate
    {
        set
        {
            if (IsRoomEngineReady)
            {
                _roomEngine.DisableUpdate = value;
            }
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::get previewRoomId
    public int PreviewRoomId { get; }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::get isRoomEngineReady
    public bool IsRoomEngineReady => _roomEngine is { IsInitialized: true };

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::dispose
    public void Dispose()
    {
        Reset(true);

        if (!IsRoomEngineReady)
        {
            return;
        }

        EventDispatcherWrapper? events = GetEvents();

        if (events == null)
        {
            return;
        }

        events.RemoveEventListener(RoomEngineObjectEvent.ADDED, _onRoomObjectAdded);
        events.RemoveEventListener(RoomEngineObjectEvent.CONTENT_UPDATED, _onRoomObjectAdded);
        events.RemoveEventListener(RoomEngineEvent.ROOM_INITIALIZED, _onRoomInitialized);
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::createRoomForPreviews
    private void CreateRoomForPreviews()
    {
        if (!IsRoomEngineReady)
        {
            return;
        }

        int size = 7;
        RoomPlaneParser planeParser = new RoomPlaneParser();
        planeParser.InitializeTileMap(size + 2, size + 2);

        for (int y = 1; y < 1 + size; y++)
        {
            for (int x = 1; x < 1 + size; x++)
            {
                planeParser.SetTileHeight(x, y, 0);
            }
        }

        planeParser.InitializeFromTileData();
        _roomEngine.InitializeRoom(PreviewRoomId, planeParser.GetXML());
        planeParser.Dispose();
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::reset
    public void Reset(bool disposing)
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.DisposeObjectFurniture(PreviewRoomId, PREVIEW_OBJECT_ID);
            _roomEngine.DisposeObjectWallItem(PreviewRoomId, PREVIEW_OBJECT_ID);
            _roomEngine.DisposeObjectUser(PreviewRoomId, PREVIEW_OBJECT_ID);

            if (!disposing)
            {
                UpdatePreviewRoomView();
            }
        }

        _currentPreviewObjectCategory = RoomObjectCategoryEnum.MINIMUM;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::addFurnitureIntoRoom
    public int AddFurnitureIntoRoom(int typeId, IVector3d direction, IStuffData? stuffData = null, string? extras = null)
    {
        int result = -1;

        stuffData ??= new LegacyStuffData();

        if (!IsRoomEngineReady)
        {
            return result;
        }

        if (_currentPreviewObjectCategory == RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE
            && _currentPreviewObjectType == typeId)
        {
            return PREVIEW_OBJECT_ID;
        }

        Reset(false);
        _currentPreviewObjectType = typeId;
        _currentPreviewObjectCategory = RoomObjectCategoryEnum.OBJECT_CATEGORY_FURNITURE;
        _currentPreviewObjectData = "";

        if (!_roomEngine.AddObjectFurniture(
                PreviewRoomId, PREVIEW_OBJECT_ID, typeId,
                new Vector3d(PREVIEW_OBJECT_LOCATION_X, PREVIEW_OBJECT_LOCATION_Y, 0),
                direction, 0, stuffData, double.NaN, -1, 0, 0, "", true, false))
        {
            return result;
        }

        _previousAutomaticStateChangeTimeMs = Environment.TickCount64;
        _automaticStateChange = true;
        result = PREVIEW_OBJECT_ID;

        IRoomObject? roomObject = _roomEngine.GetRoomObject(PreviewRoomId, PREVIEW_OBJECT_ID, _currentPreviewObjectCategory);

        if (roomObject != null && extras != null)
        {
            (roomObject.Model as IRoomObjectModelController)?.SetString(
                RoomObjectVariableEnum.FURNITURE_EXTRAS, extras);
        }

        UpdatePreviewRoomView();

        return result;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::addWallItemIntoRoom
    public int AddWallItemIntoRoom(int typeId, IVector3d direction, string data)
    {
        int result = -1;

        if (!IsRoomEngineReady)
        {
            return result;
        }

        if (_currentPreviewObjectCategory == RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL
            && _currentPreviewObjectType == typeId
            && _currentPreviewObjectData == data)
        {
            return PREVIEW_OBJECT_ID;
        }

        Reset(false);
        _currentPreviewObjectType = typeId;
        _currentPreviewObjectCategory = RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL;
        _currentPreviewObjectData = data;

        if (!_roomEngine.AddObjectWallItem(
                PreviewRoomId, PREVIEW_OBJECT_ID, typeId,
                new Vector3d(0.5, 2.3, 1.8),
                direction, 0, data, 0, 0, "", -1, false))
        {
            return result;
        }

        _previousAutomaticStateChangeTimeMs = Environment.TickCount64;
        _automaticStateChange = true;
        return PREVIEW_OBJECT_ID;

    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::addAvatarIntoRoom
    public int AddAvatarIntoRoom(string figure, int effectId)
    {
        if (!IsRoomEngineReady)
        {
            return -1;
        }

        Reset(false);
        _currentPreviewObjectType = 1;
        _currentPreviewObjectCategory = RoomObjectCategoryEnum.OBJECT_CATEGORY_USER;
        _currentPreviewObjectData = figure;

        if (_roomEngine.AddObjectUser(
                PreviewRoomId, PREVIEW_OBJECT_ID,
                new Vector3d(PREVIEW_OBJECT_LOCATION_X, PREVIEW_OBJECT_LOCATION_Y, 0),
                new Vector3d(90, 0, 0), 135, 1, figure))
        {
            _previousAutomaticStateChangeTimeMs = Environment.TickCount64;
            _automaticStateChange = true;
            UpdateUserGesture(1);
            UpdateUserEffect(effectId);
            UpdateUserPosture("std");
        }

        UpdatePreviewRoomView();
        return PREVIEW_OBJECT_ID;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateUserPosture
    public void UpdateUserPosture(string posture, string parameter = "")
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectUserPosture(PreviewRoomId, PREVIEW_OBJECT_ID, posture, parameter);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateUserGesture
    public void UpdateUserGesture(int gesture)
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectUserGesture(PreviewRoomId, PREVIEW_OBJECT_ID, gesture);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateUserEffect
    public void UpdateUserEffect(int effectId)
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectUserEffect(PreviewRoomId, PREVIEW_OBJECT_ID, effectId);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateObjectUserFigure
    public bool UpdateObjectUserFigure(string figure, string? gender = null, string? club = null, bool isRiding = false)
    {
        if (IsRoomEngineReady)
        {
            return _roomEngine.UpdateObjectUserFigure(PreviewRoomId, PREVIEW_OBJECT_ID, figure, gender, club, isRiding);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateObjectUserAction
    public void UpdateObjectUserAction(string action, int value, string? parameter = null)
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectUserAction(PreviewRoomId, PREVIEW_OBJECT_ID, action, value, parameter);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::changeRoomObjectState
    public void ChangeRoomObjectState()
    {
        if (IsRoomEngineReady)
        {
            _automaticStateChange = false;

            if (_currentPreviewObjectCategory != RoomObjectCategoryEnum.OBJECT_CATEGORY_USER)
            {
                _roomEngine.ChangeObjectState(PreviewRoomId, PREVIEW_OBJECT_ID, _currentPreviewObjectCategory);
            }
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::getRoomCanvas
    public Node2D? GetRoomCanvas(int width, int height)
    {
        if (_roomEngine == null)
        {
            return null;
        }

        Node2D? canvas = _roomEngine.CreateRoomCanvas(PreviewRoomId, PREVIEW_CANVAS_ID, width, height, _currentPreviewScale);
        _roomEngine.SetRoomCanvasMask(PreviewRoomId, PREVIEW_CANVAS_ID, true);

        IRoomGeometry? geometry = _roomEngine.GetRoomCanvasGeometry(PreviewRoomId, PREVIEW_CANVAS_ID);

        geometry?.AdjustLocation(new Vector3d(PREVIEW_OBJECT_LOCATION_X, PREVIEW_OBJECT_LOCATION_Y, 0), 30);

        _currentPreviewCanvasWidth = width;
        _currentPreviewCanvasHeight = height;
        return canvas;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::modifyRoomCanvas
    public void ModifyRoomCanvas(int width, int height)
    {
        if (_roomEngine == null)
        {
            return;
        }

        _currentPreviewCanvasWidth = width;
        _currentPreviewCanvasHeight = height;
        _roomEngine.ModifyRoomCanvas(PreviewRoomId, PREVIEW_CANVAS_ID, width, height);
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::zoomIn
    public void ZoomIn()
    {
        if (IsRoomEngineReady)
        {
            if ((_roomEngine as Component)!.GetBoolean("zoom.enabled"))
            {
                _roomEngine.SetRoomCanvasScale(PreviewRoomId, PREVIEW_CANVAS_ID, 1);
            }

            IRoomGeometry? geometry = _roomEngine.GetRoomCanvasGeometry(PreviewRoomId, PREVIEW_CANVAS_ID);
            geometry?.PerformZoomIn();
        }

        _currentPreviewScale = SCALE_NORMAL;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::zoomOut
    public void ZoomOut()
    {
        if (IsRoomEngineReady)
        {
            if ((_roomEngine as Component)!.GetBoolean("zoom.enabled"))
            {
                _roomEngine.SetRoomCanvasScale(PreviewRoomId, PREVIEW_CANVAS_ID, 0.5);
            }
            else
            {
                IRoomGeometry? geometry = _roomEngine.GetRoomCanvasGeometry(PreviewRoomId, PREVIEW_CANVAS_ID);
                geometry?.PerformZoomOut();
            }
        }

        _currentPreviewScale = SCALE_SMALL;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateAvatarDirection
    public void UpdateAvatarDirection(int bodyDirection, int headDirection)
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectUser(
                PreviewRoomId, PREVIEW_OBJECT_ID,
                new Vector3d(PREVIEW_OBJECT_LOCATION_X, PREVIEW_OBJECT_LOCATION_Y, 0),
                new Vector3d(PREVIEW_OBJECT_LOCATION_X, PREVIEW_OBJECT_LOCATION_Y, 0),
                false, 0,
                new Vector3d(bodyDirection * 45, 0, 0),
                headDirection * 45);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateObjectRoom
    public bool UpdateObjectRoom(string? floorType = null, string? wallType = null,
        string? landscapeType = null, bool animate = false)
    {
        if (IsRoomEngineReady)
        {
            return _roomEngine.UpdateObjectRoom(PreviewRoomId, floorType, wallType, landscapeType, false);
        }

        return false;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateRoomWallsAndFloorVisibility
    public void UpdateRoomWallsAndFloorVisibility(bool wallsVisible, bool floorVisible = true)
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectRoomVisibilities(PreviewRoomId, wallsVisible, floorVisible);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updatePreviewRoomView
    public void UpdatePreviewRoomView(bool force = false)
    {
        if (_disableUpdate && !force)
        {
            return;
        }

        CheckAutomaticRoomObjectStateChange();

        if (!IsRoomEngineReady)
        {
            return;
        }

        Vector2? screenOffset = _roomEngine.GetRoomCanvasScreenOffset(PreviewRoomId, PREVIEW_CANVAS_ID);

        if (screenOffset == null)
        {
            return;
        }

        Vector2 offset = screenOffset.Value;

        UpdatePreviewObjectBoundingRectangle(offset);

        if (_currentPreviewRectangle == null)
        {
            return;
        }

        int previousScale = _currentPreviewScale;
        offset = ValidatePreviewSize(offset);
        Vector2? canvasOffset = GetCanvasOffset(offset);

        if (canvasOffset != null)
        {
            _roomEngine.SetRoomCanvasScreenOffset(PreviewRoomId, PREVIEW_CANVAS_ID, canvasOffset.Value);
        }

        if (_currentPreviewScale != previousScale)
        {
            _currentPreviewRectangle = null;
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updateRoomEngine
    public void UpdateRoomEngine()
    {
        if (IsRoomEngineReady)
        {
            _roomEngine.RunUpdate();
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::getGenericRoomObjectImage
    public ImageResult? GetGenericRoomObjectImage(string type, string value, IVector3d direction,
        int scale, IGetImageListener? listener, uint bgColor = 0, string? extra = null,
        IStuffData? stuffData = null, int state = -1, int frameCount = -1, string? posture = null)
    {
        if (IsRoomEngineReady)
        {
            return _roomEngine.GetGenericRoomObjectImage(type, value, direction, scale, listener, bgColor, extra, stuffData, state,
                frameCount, posture);
        }

        return null;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::getRoomObjectImage
    public ImageResult? GetRoomObjectImage(int category, IVector3d direction, int scale,
        IGetImageListener? listener, uint bgColor = 0)
    {
        if (IsRoomEngineReady)
        {
            return _roomEngine.GetRoomObjectImage(PreviewRoomId, PREVIEW_OBJECT_ID, category, direction, scale, listener, bgColor);
        }

        return null;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::getRoomObjectCurrentImage
    public Image? GetRoomObjectCurrentImage()
    {
        if (!IsRoomEngineReady)
        {
            return null;
        }

        IRoomObject? roomObject = _roomEngine.GetRoomObject(
            PreviewRoomId, PREVIEW_OBJECT_ID, RoomObjectCategoryEnum.OBJECT_CATEGORY_USER);

        IRoomObjectVisualization? visualization = roomObject?.Visualization;

        return visualization?.GetImage(0xFFFFFF, -1);
    }

    #region Private Methods
    /// @see com.sulake.habbo.room.preview.RoomPreviewer::checkAutomaticRoomObjectStateChange
    private void CheckAutomaticRoomObjectStateChange()
    {
        if (!_automaticStateChange)
        {
            return;
        }

        long currentMs = Environment.TickCount64;

        if (currentMs <= _previousAutomaticStateChangeTimeMs + AUTOMATIC_STATE_CHANGE_INTERVAL)
        {
            return;
        }

        _previousAutomaticStateChangeTimeMs = currentMs;

        if (IsRoomEngineReady)
        {
            _roomEngine.ChangeObjectState(PreviewRoomId, PREVIEW_OBJECT_ID, _currentPreviewObjectCategory);
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::updatePreviewObjectBoundingRectangle
    private void UpdatePreviewObjectBoundingRectangle(Vector2 screenOffset)
    {
        Rect2? boundingRect = _roomEngine.GetRoomObjectBoundingRectangle(
            PreviewRoomId, PREVIEW_OBJECT_ID, _currentPreviewObjectCategory, PREVIEW_CANVAS_ID);

        if (boundingRect == null)
        {
            return;
        }

        Rect2 rect = boundingRect.Value;
        rect.Position += new Vector2(-(_currentPreviewCanvasWidth >> 1), -(_currentPreviewCanvasHeight >> 1));
        rect.Position += new Vector2(-screenOffset.X, -screenOffset.Y);

        if (_currentPreviewRectangle == null)
        {
            _currentPreviewRectangle = rect;
        }
        else
        {
            Rect2 merged = _currentPreviewRectangle.Value.Merge(rect);
            Rect2 current = _currentPreviewRectangle.Value;

            if (merged.Size.X - current.Size.X > ((_currentPreviewCanvasWidth - current.Size.X) / 2)
                || merged.Size.Y - current.Size.Y > ((_currentPreviewCanvasHeight - current.Size.Y) / 2)
                || current.Size.X < 1
                || current.Size.Y < 1)
            {
                _currentPreviewRectangle = merged;
            }
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::validatePreviewSize
    private Vector2 ValidatePreviewSize(Vector2 offset)
    {
        Rect2 rect = _currentPreviewRectangle!.Value;

        if (rect.Size.X < 1 || rect.Size.Y < 1 || !IsRoomEngineReady)
        {
            return offset;
        }

        IRoomGeometry? geometry = _roomEngine.GetRoomCanvasGeometry(PreviewRoomId, PREVIEW_CANVAS_ID);

        if (rect.Size.X > _currentPreviewCanvasWidth * (1 + ALLOWED_IMAGE_CUT)
            || rect.Size.Y > _currentPreviewCanvasHeight * (1 + ALLOWED_IMAGE_CUT))
        {
            // Object too large — zoom out
            if ((_roomEngine as Component)!.GetBoolean("zoom.enabled"))
            {
                if (_roomEngine.GetRoomCanvasScale(PreviewRoomId, PREVIEW_CANVAS_ID) == 0.5)
                {
                    return offset;
                }

                _roomEngine.SetRoomCanvasScale(PreviewRoomId, PREVIEW_CANVAS_ID, 0.5,
                    null, null, false, false, true);
                _currentPreviewScale = SCALE_SMALL;
                _currentPreviewNeedsZoomOut = true;
                offset = new Vector2(offset.X / 2, offset.Y / 2);

                Vector2 pos = rect.Position;
                Vector2 end = rect.End;
                _currentPreviewRectangle = new Rect2(
                    new Vector2(pos.X / 4, pos.Y / 4),
                    new Vector2((end.X / 4) - (pos.X / 4), (end.Y / 4) - (pos.Y / 4)));
            }
            else if (geometry != null && geometry.IsZoomedIn())
            {
                geometry.PerformZoomOut();
                _currentPreviewScale = SCALE_SMALL;
                _currentPreviewNeedsZoomOut = true;
                offset = new Vector2(offset.X / 2, offset.Y / 2);

                Vector2 pos = rect.Position;
                Vector2 end = rect.End;
                _currentPreviewRectangle = new Rect2(
                    new Vector2(pos.X / 4, pos.Y / 4),
                    new Vector2((end.X / 4) - (pos.X / 4), (end.Y / 4) - (pos.Y / 4)));
            }
        }
        else if (rect.Size.X * 2 < (_currentPreviewCanvasWidth * (1 + ALLOWED_IMAGE_CUT)) - 5
                 && rect.Size.Y * 2 < (_currentPreviewCanvasHeight * (1 + ALLOWED_IMAGE_CUT)) - 5)
        {
            // Object small enough — zoom in
            if ((_roomEngine as Component)!.GetBoolean("zoom.enabled"))
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_roomEngine.GetRoomCanvasScale(PreviewRoomId, PREVIEW_CANVAS_ID) == 1
                    || _currentPreviewNeedsZoomOut)
                {
                    return offset;
                }

                _roomEngine.SetRoomCanvasScale(PreviewRoomId, PREVIEW_CANVAS_ID, 1,
                    null, null, false, false, true);

                _currentPreviewScale = SCALE_NORMAL;
                offset = new Vector2(offset.X * 2, offset.Y * 2);
            }
            else if (geometry != null && !geometry.IsZoomedIn() && !_currentPreviewNeedsZoomOut)
            {
                geometry.PerformZoomIn();
                _currentPreviewScale = SCALE_NORMAL;
                offset = new Vector2(offset.X * 2, offset.Y * 2);
            }
        }

        return offset;
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::getCanvasOffset
    private Vector2? GetCanvasOffset(Vector2 offset)
    {
        Rect2 rect = _currentPreviewRectangle!.Value;

        if (rect.Size.X < 1 || rect.Size.Y < 1)
        {
            return offset;
        }

        double targetX = -(rect.Position.X + rect.End.X) / 2;
        double targetY = -(rect.Position.Y + rect.End.Y) / 2;

        double verticalSlack = (_currentPreviewCanvasHeight - rect.Size.Y) / 2;

        if (verticalSlack > 10)
        {
            targetY += Math.Min(15, verticalSlack - 10);
        }
        else if (_currentPreviewObjectCategory != RoomObjectCategoryEnum.OBJECT_CATEGORY_USER)
        {
            targetY += 5 - Math.Max(0, verticalSlack / 2);
        }
        else
        {
            targetY -= 5 - Math.Min(0, verticalSlack / 2);
        }

        targetY += _addViewOffset.Y;
        targetX += _addViewOffset.X;

        double diffX = targetX - offset.X;
        double diffY = targetY - offset.Y;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (diffX == 0 && diffY == 0)
        {
            return null;
        }

        double distance = Math.Sqrt((diffX * diffX) + (diffY * diffY));

        if (!(distance > 10))
        {
            return new Vector2((float)targetX, (float)targetY);
        }

        targetX = offset.X + (diffX * 10 / distance);
        targetY = offset.Y + (diffY * 10 / distance);

        return new Vector2((float)targetX, (float)targetY);

    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::onRoomInitialized
    private void OnRoomInitialized(object? eventObj)
    {
        if (eventObj is not RoomEngineEvent engineEvent)
        {
            return;
        }

        if (engineEvent.Type != RoomEngineEvent.ROOM_INITIALIZED || engineEvent.RoomId != PreviewRoomId)
        {
            return;
        }

        if (IsRoomEngineReady)
        {
            _roomEngine.UpdateObjectRoom(PreviewRoomId, "110", "99999");
        }
    }

    /// @see com.sulake.habbo.room.preview.RoomPreviewer::onRoomObjectAdded
    private void OnRoomObjectAdded(object? eventObj)
    {
        if (eventObj is not RoomEngineObjectEvent objectEvent)
        {
            return;
        }

        if (objectEvent.RoomId != PreviewRoomId
            || objectEvent.ObjectId != PREVIEW_OBJECT_ID
            || objectEvent.Category != _currentPreviewObjectCategory)
        {
            return;
        }

        _currentPreviewRectangle = null;
        _currentPreviewNeedsZoomOut = false;

        IRoomObject? roomObject = _roomEngine.GetRoomObject(objectEvent.RoomId, objectEvent.ObjectId, objectEvent.Category);

        if (roomObject?.Model == null || objectEvent.Category != RoomObjectCategoryEnum.OBJECT_CATEGORY_WALL)
        {
            return;
        }

        double sizeZ = roomObject.Model.GetNumber(RoomObjectVariableEnum.FURNITURE_SIZE_Z);
        double centerZ = roomObject.Model.GetNumber(RoomObjectVariableEnum.FURNITURE_CENTER_Z);

        if (!double.IsNaN(sizeZ) && !double.IsNaN(centerZ))
        {
            _roomEngine.UpdateObjectWallItemLocation(
                objectEvent.RoomId, objectEvent.ObjectId,
                new Vector3d(0.5, 2.3, ((3.6 - sizeZ) / 2) + centerZ));
        }
    }

    private EventDispatcherWrapper? GetEvents()
    {
        return _roomEngine.Events as EventDispatcherWrapper;
    }
    #endregion
}
