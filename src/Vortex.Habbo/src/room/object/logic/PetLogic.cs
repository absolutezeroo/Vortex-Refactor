using System;
using System.Xml.Linq;

using Vortex.Habbo.Avatar.Pets;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.PetLogic
public class PetLogic : MovingObjectLogic
{
    private int _talkEndTime;
    private int _gestureEndTime;
    private int _expressionEndTime;
    private bool _selected;
    private Vector3d? _lastKnownPosition;
    private readonly bool _debugMode;
    private int _debugPostureIndex;
    private int _debugGestureIndex;
    private int _headDirectionDelta;
    private int _debugDirectionIndex;
    private List<int> _directions = [];

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK, RoomObjectMoveEvent.POSITION_CHANGED];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        if (_selected && Object != null && EventDispatcher != null)
        {
            DispatchEvent(new RoomObjectMoveEvent(RoomObjectMoveEvent.OBJECT_REMOVED, Object));
        }

        _directions = null!;

        base.Dispose();

        _lastKnownPosition = null;
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        _directions = [];

        IEnumerable<XElement>? directionElements = xml.Element("model")?.Element("directions")?.Elements("direction");

        if (directionElements != null)
        {
            string[] requiredAttrs = ["id"];

            foreach (XElement dirElem in directionElements)
            {
                if (!XMLValidator.CheckRequiredAttributes(dirElem, requiredAttrs))
                {
                    continue;
                }

                XAttribute? idAttr = dirElem.Attribute("id");

                if (idAttr != null && int.TryParse(idAttr.Value, out int id))
                {
                    _directions.Add(id);
                }
            }
        }

        _directions.Sort();
        Object.ModelController.SetNumberArray("pet_allowed_directions", _directions, true);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        if (message == null || Object == null)
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;

        if (!_debugMode)
        {
            base.ProcessUpdateMessage(message);

            switch (message)
            {
                case RoomObjectAvatarPostureUpdateMessage postureMsg:
                    model.SetString("figure_posture", postureMsg.PostureType);
                    return;
                case RoomObjectAvatarUpdateMessage avatarMsg:
                    model.SetNumber("head_direction", avatarMsg.DirHead);
                    return;
                case RoomObjectAvatarDirectionUpdateMessage dirMsg:
                    model.SetNumber("head_direction", dirMsg.DirHead);
                    return;
                case RoomObjectAvatarChatUpdateMessage chatMsg:
                    model.SetNumber("figure_talk", 1);
                    _talkEndTime = Environment.TickCount + (chatMsg.NumberOfWords * 1000);
                    return;
                case RoomObjectAvatarPetGestureUpdateMessage petGestureMsg:
                    model.SetString("figure_gesture", petGestureMsg.Gesture);
                    _gestureEndTime = Environment.TickCount + 3000;
                    return;
                case RoomObjectAvatarSleepUpdateMessage sleepMsg:
                    model.SetNumber("figure_sleep", sleepMsg.IsSleeping ? 1 : 0);
                    return;
            }

        }

        switch (message)
        {
            case RoomObjectAvatarSelectedMessage selectedMsg:
                _selected = selectedMsg.Selected;
                _lastKnownPosition = null;
                return;
            case RoomObjectAvatarExperienceUpdateMessage expMsg:
                model.SetNumber("figure_experience_timestamp", Environment.TickCount);
                model.SetNumber("figure_gained_experience", expMsg.GainedExperience);
                return;
            case RoomObjectAvatarFigureUpdateMessage figureMsg:
                {
                    PetFigureData petData = new(figureMsg.Figure);
                    model.SetString("figure", figureMsg.Figure);
                    model.SetString("race", figureMsg.Race ?? "");
                    model.SetNumber("pet_palette_index", petData.PaletteId);
                    model.SetNumber("pet_color", petData.Color);
                    model.SetNumber("pet_type", petData.TypeId);
                    model.SetNumberArray("pet_custom_layer_ids", new List<int>(petData.CustomLayerIds));
                    model.SetNumberArray("pet_custom_part_ids", new List<int>(petData.CustomPartIds));
                    model.SetNumberArray("pet_custom_palette_ids", new List<int>(petData.CustomPaletteIds));
                    model.SetNumber("pet_is_riding", figureMsg.IsRiding ? 1 : 0);
                    break;
                }
        }

    }

    public override void MouseEvent(RoomSpriteMouseEvent? mouseEvent, IRoomGeometry geometry)
    {
        if (Object == null || mouseEvent == null)
        {
            return;
        }

        IRoomObjectModelController model = Object.ModelController;
        string? eventType = null;

        switch (mouseEvent.Type)
        {
            case "click":
                eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK;
                if (_debugMode)
                {
                    DebugMouseEvent(mouseEvent);
                }
                break;

            case "doubleClick":
                break;

            case "mouseDown":
                if (!_debugMode)
                {
                    int petType = (int)model.GetNumber("pet_type");
                    if (petType == 16 && EventDispatcher != null)
                    {
                        DispatchEvent(
                            new RoomObjectMouseEvent(
                                RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_DOWN, Object,
                                mouseEvent.EventId, mouseEvent.AltKey, mouseEvent.CtrlKey,
                                mouseEvent.ShiftKey, mouseEvent.ButtonDown
                            )
                        );
                    }
                }
                break;
        }

        if (eventType != null && EventDispatcher != null)
        {
            DispatchEvent(
                new RoomObjectMouseEvent(
                    eventType, Object, mouseEvent.EventId,
                    mouseEvent.AltKey, mouseEvent.CtrlKey, mouseEvent.ShiftKey, mouseEvent.ButtonDown
                )
            );
        }
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (_selected && Object != null && EventDispatcher != null)
        {
            IVector3d loc = Object.Location;
            if (_lastKnownPosition == null ||
                _lastKnownPosition.X != loc.X ||
                _lastKnownPosition.Y != loc.Y ||
                _lastKnownPosition.Z != loc.Z)
            {
                _lastKnownPosition ??= new Vector3d();
                _lastKnownPosition.Assign(loc);
                DispatchEvent(new RoomObjectMoveEvent(RoomObjectMoveEvent.POSITION_CHANGED, Object));
            }
        }

        if (Object?.ModelController != null)
        {
            UpdateActions(time, Object.ModelController);
        }
    }

    private void DebugMouseEvent(RoomSpriteMouseEvent mouseEvent)
    {
        IRoomObjectModelController model = Object!.ModelController;

        switch (mouseEvent)
        {
            case
            {
                AltKey: false,
                CtrlKey: false,
            } when _directions.Count <= 0:
                return;
            case
            {
                AltKey: false,
                CtrlKey: false,
            }:
                {
                    int dir = _directions[_debugDirectionIndex];
                    Object.SetDirection(new Vector3d(dir));
                    model.SetNumber("head_direction", dir + _headDirectionDelta);
                    _debugDirectionIndex++;
                    if (_debugDirectionIndex == _directions.Count)
                    {
                        _debugDirectionIndex = 0;
                    }
                    break;
                }
            case
            {
                AltKey: true,
                CtrlKey: false,
            }:
                _debugPostureIndex++;
                model.SetNumber("figure_posture", _debugPostureIndex);
                model.SetNumber("figure_gesture", double.NaN);
                break;
            case
            {
                CtrlKey: true,
                AltKey: false,
            }:
                _debugGestureIndex++;
                model.SetNumber("figure_gesture", _debugGestureIndex);
                break;
            default:
                {
                    _headDirectionDelta += 45;
                    if (_headDirectionDelta > 45)
                    {
                        _headDirectionDelta = -45;
                    }
                    int dir = (int)Object.Direction.X;
                    model.SetNumber("head_direction", dir + _headDirectionDelta);
                    break;
                }
        }
    }

    private void UpdateActions(int time, IRoomObjectModelController model)
    {
        if (_gestureEndTime > 0 && time > _gestureEndTime)
        {
            model.SetString("figure_gesture", null!);
            _gestureEndTime = 0;
        }

        if (_talkEndTime > 0)
        {
            if (time > _talkEndTime)
            {
                model.SetNumber("figure_talk", 0);
                _talkEndTime = 0;
            }
        }

        if (_expressionEndTime <= 0 || time <= _expressionEndTime)
        {
            return;
        }

        model.SetNumber("figure_expression", 0);

        _expressionEndTime = 0;
    }
}
