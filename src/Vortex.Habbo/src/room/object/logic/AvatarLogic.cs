using System;

using Vortex.Habbo.Avatar.Enum;
using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Object;
using Vortex.Room.Utils;

namespace Vortex.Habbo.Room.Object.Logic;

/// @see com.sulake.habbo.room.object.logic.AvatarLogic
public class AvatarLogic : MovingObjectLogic
{
    private const double WARP_DISTANCE = 1.5;
    private const int EFFECT_TYPE_SPLASH = 28;
    private const int EFFECT_TYPE_SWIM = 29;
    private const int EFFECT_TYPE_SPLASH_ALT = 184;
    private const int EFFECT_TYPE_SWIM_ALT = 185;
    private const int EFFECT_SPLASH_LENGTH = 500;
    private const int CARRY_ITEM_NULL = 0;
    private const int CARRY_ITEM_LAST_CONSUMABLE = 999;
    private const int CARRY_ITEM_EMPTY_HAND = 999999999;
    private const int CARRY_ITEM_DELAY_BEFORE_USE = 5000;
    private const int CARRY_ITEM_EMPTY_HAND_ANIMATION_LENGTH = 1500;

    private bool _selected;
    private Vector3d? _lastKnownPosition;
    private int _effectChangeTime;
    private int _nextEffectId;
    private int _talkEndTime;
    private int _talkPauseNextTime;
    private int _talkResumeNextTime;
    private int _expressionEndTime;
    private int _gestureEndTime;
    private int _signEndTime;
    private int _carryStartTime;
    private int _carryEmptyEndTime;
    private bool _allowUseCarryObject;
    private int _blinkEndTime;
    private int _nextBlinkTime = Environment.TickCount + GetBlinkInterval();
    private int _playerValueEndTime;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK,
            RoomObjectMoveEvent.POSITION_CHANGED,
            RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_ENTER,
            RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_LEAVE,
            RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON,
            RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW,
        ];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        if (_selected && Object != null)
        {
            if (EventDispatcher != null)
            {
                DispatchEvent(new RoomObjectMoveEvent(RoomObjectMoveEvent.OBJECT_REMOVED, Object));
            }
        }
        base.Dispose();
        _lastKnownPosition = null;
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        if (message == null || Object == null)
        {
            return;
        }

        base.ProcessUpdateMessage(message);

        IRoomObjectModelController model = Object.ModelController;

        switch (message)
        {
            case RoomObjectAvatarPostureUpdateMessage postureMsg:
                model.SetString("figure_posture", postureMsg.PostureType);
                model.SetString("figure_posture_parameter", postureMsg.Parameter);
                return;
            case RoomObjectAvatarChatUpdateMessage chatMsg:
                model.SetNumber("figure_talk", 1);
                _talkEndTime = Environment.TickCount + (chatMsg.NumberOfWords * 1000);
                return;
            case RoomObjectAvatarTypingUpdateMessage typingMsg:
                model.SetNumber("figure_is_typing", typingMsg.IsTyping ? 1 : 0);
                return;
            case RoomObjectAvatarMutedUpdateMessage mutedMsg:
                model.SetNumber("figure_is_muted", mutedMsg.IsMuted ? 1 : 0);
                return;
            case RoomObjectAvatarPlayingGameMessage gameMsg:
                model.SetNumber("figure_is_playing_game", gameMsg.IsPlayingGame ? 1 : 0);
                return;
            case RoomObjectAvatarUpdateMessage avatarMsg:
                model.SetNumber("head_direction", avatarMsg.DirHead);
                model.SetNumber("figure_can_stand_up", avatarMsg.CanStandUp ? 1 : 0);
                model.SetNumber("figure_vertical_offset", avatarMsg.BaseY);
                return;
            case RoomObjectAvatarDirectionUpdateMessage dirMsg:
                model.SetNumber("head_direction", dirMsg.DirHead);
                return;
            case RoomObjectAvatarGestureUpdateMessage gestureMsg:
                model.SetNumber("figure_gesture", gestureMsg.Gesture);
                _gestureEndTime = Environment.TickCount + 3000;
                return;
            case RoomObjectAvatarExpressionUpdateMessage exprMsg:
                {
                    model.SetNumber("figure_expression", exprMsg.ExpressionType);
                    _expressionEndTime = AvatarExpressionEnum.GetExpressionTime((int)model.GetNumber("figure_expression"));

                    if (_expressionEndTime > -1)
                    {
                        _expressionEndTime += Environment.TickCount;
                    }

                    return;
                }
            case RoomObjectAvatarDanceUpdateMessage danceMsg:
                model.SetNumber("figure_dance", danceMsg.DanceStyle);
                return;
            case RoomObjectAvatarSleepUpdateMessage sleepMsg:
                model.SetNumber("figure_sleep", sleepMsg.IsSleeping ? 1 : 0);
                return;
            case RoomObjectAvatarPlayerValueUpdateMessage playerValueMsg:
                model.SetNumber("figure_number_value", playerValueMsg.Value);
                _playerValueEndTime = Environment.TickCount + 3000;
                return;
            case RoomObjectAvatarEffectUpdateMessage effectMsg:
                UpdateEffect(effectMsg.Effect, effectMsg.DelayMilliSeconds, model);
                return;
            case RoomObjectAvatarCarryObjectUpdateMessage carryMsg:
                {
                    model.SetNumber("figure_carry_object", carryMsg.ItemType);
                    model.SetNumber("figure_use_object", 0);
                    _carryStartTime = Environment.TickCount;

                    if (carryMsg.ItemType < CARRY_ITEM_EMPTY_HAND)
                    {
                        _carryEmptyEndTime = 0;
                        _allowUseCarryObject = carryMsg.ItemType <= CARRY_ITEM_LAST_CONSUMABLE;
                    }
                    else
                    {
                        _carryEmptyEndTime = _carryStartTime + CARRY_ITEM_EMPTY_HAND_ANIMATION_LENGTH;
                        _allowUseCarryObject = false;
                    }

                    return;
                }
            case RoomObjectAvatarUseObjectUpdateMessage useMsg:
                model.SetNumber("figure_use_object", useMsg.ItemType);
                return;
            case RoomObjectAvatarSignUpdateMessage signMsg:
                model.SetNumber("figure_sign", signMsg.SignType);
                _signEndTime = Environment.TickCount + 5000;
                return;
            case RoomObjectAvatarFlatControlUpdateMessage flatMsg:
                {
                    if (int.TryParse(flatMsg.RawData, out int level) && level is >= 0 and <= 5)
                    {
                        model.SetNumber("figure_flat_control", level);
                    }
                    else
                    {
                        model.SetNumber("figure_flat_control", 0);
                    }
                    return;
                }
            case RoomObjectAvatarFigureUpdateMessage figureMsg:
                {
                    string currentFigure = model.GetString("figure") ?? "";
                    string newFigure = figureMsg.Figure;
                    string? gender = figureMsg.Gender;

                    if (currentFigure.Contains(".bds-"))
                    {
                        int bdsIndex = currentFigure.IndexOf(".bds-", StringComparison.Ordinal);
                        newFigure += currentFigure[bdsIndex..];
                    }

                    model.SetString("figure", newFigure);
                    model.SetString("gender", gender ?? "");
                    return;
                }
            case RoomObjectAvatarSelectedMessage selectedMsg:
                _selected = selectedMsg.Selected;
                _lastKnownPosition = null;
                return;
            case RoomObjectAvatarGuideStatusUpdateMessage guideMsg:
                model.SetNumber("figure_guide_status", guideMsg.GuideStatus);
                return;
            case RoomObjectAvatarOwnMessage:
                model.SetNumber("own_user", 1);
                break;
        }

    }

    public override void MouseEvent(RoomSpriteMouseEvent? mouseEvent, IRoomGeometry geometry)
    {
        if (Object == null || mouseEvent == null)
        {
            return;
        }

        IRoomObjectModelController? model = Object.ModelController;
        string? eventType = null;

        switch (mouseEvent.Type)
        {
            case "click":
                eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_CLICK;
                break;

            case "rollOver":
                eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_ENTER;
                model?.SetNumber("figure_highlight", 1);
                DispatchEvent(
                    new RoomObjectFurnitureActionEvent(
                        RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON, Object
                    )
                );
                break;

            case "rollOut":
                model?.SetNumber("figure_highlight", 0);
                eventType = RoomObjectMouseEvent.ROOM_OBJECT_MOUSE_LEAVE;
                DispatchEvent(
                    new RoomObjectFurnitureActionEvent(
                        RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW, Object
                    )
                );
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

        if (Object == null)
        {
            return;
        }

        IRoomObjectModelController? model = Object.ModelController;

        if (model != null)
        {
            UpdateActions(time, model);
        }
    }

    private void UpdateEffect(int effect, int delayMs, IRoomObjectModelController model)
    {
        switch (effect)
        {
            case EFFECT_TYPE_SPLASH:
                _effectChangeTime = Environment.TickCount + EFFECT_SPLASH_LENGTH;
                _nextEffectId = EFFECT_TYPE_SWIM;
                break;
            case EFFECT_TYPE_SPLASH_ALT:
                _effectChangeTime = Environment.TickCount + EFFECT_SPLASH_LENGTH;
                _nextEffectId = EFFECT_TYPE_SWIM_ALT;
                break;
            default:
                {
                    if ((int)model.GetNumber("figure_effect") == EFFECT_TYPE_SWIM)
                    {
                        _effectChangeTime = Environment.TickCount + EFFECT_SPLASH_LENGTH;
                        _nextEffectId = effect;
                        effect = EFFECT_TYPE_SPLASH;
                    }
                    else if ((int)model.GetNumber("figure_effect") == EFFECT_TYPE_SWIM_ALT)
                    {
                        _effectChangeTime = Environment.TickCount + EFFECT_SPLASH_LENGTH;
                        _nextEffectId = effect;
                        effect = EFFECT_TYPE_SPLASH_ALT;
                    }
                    else
                    {
                        if (delayMs != 0)
                        {
                            _effectChangeTime = Environment.TickCount + delayMs;
                            _nextEffectId = effect;
                            return;
                        }
                        _effectChangeTime = 0;
                    }
                    break;
                }
        }

        model.SetNumber("figure_effect", effect);
    }

    private void UpdateActions(int time, IRoomObjectModelController model)
    {
        if (_talkEndTime > 0)
        {
            if (time > _talkEndTime)
            {
                model.SetNumber("figure_talk", 0);
                _talkEndTime = 0;
                _talkResumeNextTime = 0;
                _talkPauseNextTime = 0;
            }
            else if (_talkPauseNextTime == 0 && _talkResumeNextTime == 0)
            {
                _talkResumeNextTime = time + GetTalkingPauseInterval();
                _talkPauseNextTime = _talkResumeNextTime + GetTalkingPauseLength();
            }
            else if (_talkResumeNextTime > 0 && time > _talkResumeNextTime)
            {
                model.SetNumber("figure_talk", 0);
                _talkResumeNextTime = 0;
            }
            else if (_talkPauseNextTime > 0 && time > _talkPauseNextTime)
            {
                model.SetNumber("figure_talk", 1);
                _talkPauseNextTime = 0;
            }
        }

        if (_expressionEndTime > 0 && time > _expressionEndTime)
        {
            model.SetNumber("figure_expression", 0);
            _expressionEndTime = 0;
        }

        if (_gestureEndTime > 0 && time > _gestureEndTime)
        {
            model.SetNumber("figure_gesture", 0);
            _gestureEndTime = 0;
        }

        if (_signEndTime > 0 && time > _signEndTime)
        {
            model.SetNumber("figure_sign", -1);
            _signEndTime = 0;
        }

        if (_carryEmptyEndTime > 0)
        {
            if (time > _carryEmptyEndTime)
            {
                model.SetNumber("figure_carry_object", 0);
                model.SetNumber("figure_use_object", 0);
                _carryStartTime = 0;
                _carryEmptyEndTime = 0;
                _allowUseCarryObject = false;
            }
        }

        if (_allowUseCarryObject)
        {
            if (time - _carryStartTime > CARRY_ITEM_DELAY_BEFORE_USE)
            {
                if ((time - _carryStartTime) % 10000 < 1000)
                {
                    model.SetNumber("figure_use_object", 1);
                }
                else
                {
                    model.SetNumber("figure_use_object", 0);
                }
            }
        }

        if (time > _nextBlinkTime)
        {
            model.SetNumber("figure_blink", 1);
            _nextBlinkTime = time + GetBlinkInterval();
            _blinkEndTime = time + GetBlinkLength();
        }

        if (_blinkEndTime > 0 && time > _blinkEndTime)
        {
            model.SetNumber("figure_blink", 0);
            _blinkEndTime = 0;
        }

        if (_effectChangeTime > 0 && time > _effectChangeTime)
        {
            model.SetNumber("figure_effect", _nextEffectId);
            _effectChangeTime = 0;
        }

        if (_playerValueEndTime > 0 && time > _playerValueEndTime)
        {
            model.SetNumber("figure_number_value", 0);
            _playerValueEndTime = 0;
        }
    }

    private static int GetTalkingPauseInterval()
    {
        return 100 + (int)(Random.Shared.NextDouble() * 200);
    }

    private static int GetTalkingPauseLength()
    {
        return 75 + (int)(Random.Shared.NextDouble() * 75);
    }

    private static int GetBlinkInterval()
    {
        return 4500 + (int)(Random.Shared.NextDouble() * 1000);
    }

    private static int GetBlinkLength()
    {
        return 50 + (int)(Random.Shared.NextDouble() * 200);
    }
}
