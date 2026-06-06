namespace Vortex.Habbo.Room.Object.Logic;

using Events;
using Messages;
using Data;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureRoomBackgroundColorLogic (class_3457)
public class FurnitureRoomBackgroundColorLogic : FurnitureMultiStateLogic
{
    private bool _colorSet;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.BACKGROUND_COLOR,
            RoomObjectHSLColorEnableEvent.ROOM_BACKGROUND_COLOR,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        if (_colorSet && Object != null)
        {
            DispatchEvent(new RoomObjectHSLColorEnableEvent(
                RoomObjectHSLColorEnableEvent.ROOM_BACKGROUND_COLOR,
                Object, false, 0, 0, 0));
        }

        base.Dispose();
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage dataMsg || Object == null)
        {
            return;
        }

        double realRoom = Object.Model.GetNumber("furniture_real_room_object");

        if (realRoom == 1)
        {
            SetupColor(dataMsg.Data);
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.BACKGROUND_COLOR, Object));
    }

    private void SetupColor(IStuffData? data)
    {
        if (Object == null || data is not IntArrayStuffData intData)
        {
            return;
        }

        int state = intData.GetValue(0);
        int hue = intData.GetValue(1);
        int saturation = intData.GetValue(2);
        int lightness = intData.GetValue(3);

        bool enable = state != 0;
        _colorSet = enable;

        DispatchEvent(new RoomObjectHSLColorEnableEvent(
            RoomObjectHSLColorEnableEvent.ROOM_BACKGROUND_COLOR,
            Object, enable, hue, saturation, lightness));
    }
}
