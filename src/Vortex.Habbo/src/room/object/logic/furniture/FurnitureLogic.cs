using System.Linq;

namespace Vortex.Habbo.Room.Object.Logic;

using System;
using System.Xml.Linq;

using Events;
using Messages;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureLogic
public class FurnitureLogic : MovingObjectLogic
{
    private const int BOUNCE_STEPS = 8;
    private const double BOUNCE_STEP_HEIGHT = 0.0625;

    private bool _mouseOver;
    private double _sizeX;
    private double _sizeY;
    private double _sizeZ;
    private double _centerX;
    private double _centerY;
    private double _centerZ;
    private bool _directionInitialized;
    private int _bouncingStep;
    private RoomObjectUpdateMessage? _storedRotateMessage;
    private readonly Vector3d _locationOffset = new();
    private List<int>? _directions = [];

    public override IRoomObjectController? Object
    {
        get => base.Object;
        set
        {
            base.Object = value;
            if (value?.Location.Length > 0)
            {
                _directionInitialized = true;
            }
        }
    }

    public override string[]? GetEventTypes()
    {
        List<string> types =
        [
            RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_SHOW,
            RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_HIDE,
            RoomObjectRoomAdEvent.ROOM_AD_FURNI_DOUBLE_CLICK,
            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE,
            RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK,
            RoomObjectRoomAdEvent.ROOM_AD_FURNI_CLICK,
            RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_DOWN,
        ];

        if (Widget != null)
        {
            types.Add(RoomObjectWidgetRequestEvent.OPEN_WIDGET);
            types.Add(RoomObjectWidgetRequestEvent.CLOSE_WIDGET);
        }

        if (ContextMenu != null)
        {
            types.Add(RoomObjectWidgetRequestEvent.OPEN_FURNI_CONTEXT_MENU);
            types.Add(RoomObjectWidgetRequestEvent.CLOSE_FURNI_CONTEXT_MENU);
        }

        return GetAllEventTypes(base.GetEventTypes() ?? [], types.ToArray());
    }

    public override void Dispose()
    {
        base.Dispose();
        _storedRotateMessage = null;
        _directions = null;
    }

    public override void Initialize(XElement? xml)
    {
        if (xml == null || Object == null)
        {
            return;
        }

        XElement? dimensions = xml.Element("model")?.Element("dimensions");

        if (dimensions != null)
        {
            _sizeX = (double?)dimensions.Attribute("x") ?? 0;
            _sizeY = (double?)dimensions.Attribute("y") ?? 0;
            _sizeZ = (double?)dimensions.Attribute("z") ?? 0;
            _centerX = _sizeX / 2.0;
            _centerY = _sizeY / 2.0;
            _centerZ = (double?)dimensions.Attribute("centerZ") ?? (_sizeZ / 2.0);
        }

        XElement? directionsElement = xml.Element("model")?.Element("directions");

        if (directionsElement != null)
        {
            _directions ??= [];
            _directions.Clear();
            foreach (XElement dirElem in directionsElement.Elements("direction"))
            {
                int? id = (int?)dirElem.Attribute("id");
                if (id.HasValue)
                {
                    _directions.Add(id.Value);
                }
            }
            _directions.Sort();
        }

        IRoomObjectModelController model = Object.ModelController;

        XElement? customVars = xml.Element("model")?.Element("customvars");
        if (customVars != null)
        {
            List<string> varNames = [];

            varNames.AddRange(customVars.Elements("variable").Select(variable => (string?)variable.Attribute("name")).OfType<string>());
            model.SetStringArray("furniture_custom_variables", varNames.ToArray());
        }

        model.SetNumber("furniture_size_x", _sizeX);
        model.SetNumber("furniture_size_y", _sizeY);
        model.SetNumber("furniture_size_z", _sizeZ);
        model.SetNumber("furniture_center_x", _centerX);
        model.SetNumber("furniture_center_y", _centerY);
        model.SetNumber("furniture_center_z", _centerZ);
        model.SetNumberArray("furniture_allowed_directions", _directions ?? []);
        model.SetNumber("furniture_alpha_multiplier", 1);
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (Object == null)
        {
            return;
        }

        switch (mouseEvent.Type)
        {
            case "mouseMove":
                DispatchEvent(new RoomObjectMouseEvent(
                    RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_MOVE, Object,
                    mouseEvent.EventId, mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey, mouseEvent.ButtonDown));
                break;

            case "rollOver":
                if (!_mouseOver)
                {
                    string? adUrl = GetAdClickUrl(Object.ModelController);

                    if (adUrl != null && adUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        DispatchEvent(new RoomObjectRoomAdEvent(
                            RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_SHOW, Object));
                    }
                }

                DispatchEvent(new RoomObjectMouseEvent(
                    RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_ENTER, Object,
                    mouseEvent.EventId, mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey, mouseEvent.ButtonDown));

                _mouseOver = true;

                break;

            case "rollOut":
                if (_mouseOver)
                {
                    string? adUrl = GetAdClickUrl(Object.ModelController);

                    if (adUrl != null && adUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        DispatchEvent(new RoomObjectRoomAdEvent(
                            RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_HIDE, Object));
                    }
                }

                DispatchEvent(new RoomObjectMouseEvent(
                    RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_LEAVE, Object,
                    mouseEvent.EventId, mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey, mouseEvent.ButtonDown));

                _mouseOver = false;

                break;

            case "doubleClick":
                UseObject();
                break;

            case "click":
                DispatchEvent(new RoomObjectMouseEvent(
                    RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK, Object,
                    mouseEvent.EventId, mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey, mouseEvent.ButtonDown));

                string? clickAdUrl = GetAdClickUrl(Object.ModelController);

                if (clickAdUrl != null && clickAdUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    DispatchEvent(new RoomObjectRoomAdEvent(
                        RoomObjectRoomAdEvent.ROOM_AD_TOOLTIP_HIDE, Object));
                }

                if (clickAdUrl is { Length: > 0 })
                {
                    HandleAdClick(Object.Id, Object.Type ?? "", clickAdUrl);
                }

                if (ContextMenu != null)
                {
                    DispatchEvent(new RoomObjectWidgetRequestEvent(
                        RoomObjectWidgetRequestEvent.OPEN_FURNI_CONTEXT_MENU, Object));
                }
                break;

            case "mouseDown":
                DispatchEvent(new RoomObjectMouseEvent(
                    RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_DOWN, Object,
                    mouseEvent.EventId, mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey, mouseEvent.ButtonDown));

                break;
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        string? adUrl = GetAdClickUrl(Object.ModelController);

        if (adUrl is { Length: > 0 })
        {
            DispatchEvent(new RoomObjectRoomAdEvent(
                RoomObjectRoomAdEvent.ROOM_AD_FURNI_DOUBLE_CLICK, Object, "", adUrl));
        }

        if (EventDispatcher == null)
        {
            return;
        }

        if (Widget != null)
        {
            DispatchEvent(new RoomObjectWidgetRequestEvent(
                RoomObjectWidgetRequestEvent.OPEN_WIDGET, Object));
        }

        DispatchEvent(new RoomObjectStateChangeEvent(
            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object));
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        if (message == null || Object == null)
        {
            return;
        }

        switch (message)
        {
            case RoomObjectDataUpdateMessage dataMsg:
                HandleDataUpdateMessage(dataMsg);
                return;
            case RoomObjectHeightUpdateMessage heightMsg:
                HandleHeightUpdateMessage(heightMsg);
                return;
            case RoomObjectItemDataUpdateMessage itemMsg:
                HandleItemDataUpdateMessage(itemMsg);
                return;
        }

        _mouseOver = false;

        if (message is not RoomObjectMoveUpdateMessage && message is
            {
                Direction: not null,
                Location: not null,
            })
        {
            if (Object.Direction.X != message.Direction.X && _directionInitialized)
            {
                IVector3d currentLoc = Object.Location;
                if (currentLoc.X == message.Location.X &&
                    currentLoc.Y == message.Location.Y &&
                    currentLoc.Z == message.Location.Z)
                {
                    _bouncingStep = 1;
                    _storedRotateMessage = message;
                    message = null!;
                }
            }
        }

        _directionInitialized = true;

        if (message is RoomObjectSelectedMessage selectedMsg)
        {
            if (ContextMenu != null)
            {
                if (selectedMsg.Selected)
                {
                    DispatchEvent(new RoomObjectWidgetRequestEvent(
                        RoomObjectWidgetRequestEvent.OPEN_FURNI_CONTEXT_MENU, Object));
                }
                else
                {
                    DispatchEvent(new RoomObjectWidgetRequestEvent(
                        RoomObjectWidgetRequestEvent.CLOSE_FURNI_CONTEXT_MENU, Object));
                }
            }
        }

        if (message != null)
        {
            base.ProcessUpdateMessage(message);
        }
    }

    protected override IVector3d? GetLocationOffset()
    {
        if (_bouncingStep <= 0)
        {
            return null;
        }
        _locationOffset.X = 0;
        _locationOffset.Y = 0;

        int halfSteps = BOUNCE_STEPS / 2;

        if (_bouncingStep <= halfSteps)
        {
            _locationOffset.Z = BOUNCE_STEP_HEIGHT * _bouncingStep;
        }
        else if (_bouncingStep <= BOUNCE_STEPS)
        {
            if (_storedRotateMessage != null)
            {
                base.ProcessUpdateMessage(_storedRotateMessage);
                _storedRotateMessage = null;
            }
            _locationOffset.Z = BOUNCE_STEP_HEIGHT * (BOUNCE_STEPS - _bouncingStep);
        }

        return _locationOffset;

    }

    public override void Update(int time)
    {
        base.Update(time);

        if (_bouncingStep <= 0)
        {
            return;
        }

        _bouncingStep++;

        if (_bouncingStep > BOUNCE_STEPS)
        {
            _bouncingStep = 0;
        }
    }

    public override void TearDown()
    {
        if (Widget != null && Object != null)
        {
            double realRoom = Object.Model.GetNumber("furniture_real_room_object");

            if (realRoom == 1)
            {
                DispatchEvent(new RoomObjectWidgetRequestEvent(
                    RoomObjectWidgetRequestEvent.CLOSE_WIDGET, Object));
            }
        }
        base.TearDown();
    }

    protected virtual string? GetAdClickUrl(IRoomObjectModelController model)
    {
        return model.GetString("furniture_ad_url");
    }

    protected virtual void HandleAdClick(int objectId, string objectType, string clickUrl)
    {
        DispatchEvent(new RoomObjectRoomAdEvent(
            RoomObjectRoomAdEvent.ROOM_AD_FURNI_CLICK, Object));
    }

    private void HandleDataUpdateMessage(RoomObjectDataUpdateMessage message)
    {
        if (Object == null)
        {
            return;
        }

        Object.SetState(message.State, 0);

        IRoomObjectModelController model = Object.ModelController;

        message.Data?.WriteRoomObjectModel(model);

        if (!double.IsNaN(message.Extra))
        {
            model.SetNumber("furniture_extras", message.Extra);
        }

        model.SetNumber("furniture_state_update_time", LastUpdateTime);
    }

    private void HandleHeightUpdateMessage(RoomObjectHeightUpdateMessage message)
    {
        Object?.ModelController.SetNumber("furniture_size_z", message.Height);
    }

    private void HandleItemDataUpdateMessage(RoomObjectItemDataUpdateMessage message)
    {
        Object?.ModelController.SetString("furniture_itemdata", message.ItemData);
    }
}
