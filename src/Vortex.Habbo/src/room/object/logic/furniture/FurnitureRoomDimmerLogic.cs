namespace Vortex.Habbo.Room.Object.Logic;

using System;
using System.Globalization;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureRoomDimmerLogic
public class FurnitureRoomDimmerLogic : FurnitureLogic
{
    private bool _dimmerActive;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.DIMMER,
            RoomObjectWidgetRequestEvent.WIDGET_REMOVE_DIMMER,
            RoomObjectDimmerStateUpdateEvent.DIMMER_STATE,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Dispose()
    {
        if (_dimmerActive && Object != null)
        {
            DispatchEvent(new RoomObjectDimmerStateUpdateEvent(Object, 0, 0, 0, 16777215, 255));
        }

        base.Dispose();
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        double realRoom = Object.Model.GetNumber("furniture_real_room_object");

        if (realRoom == 1)
        {
            DispatchColorUpdateEvent();
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.DIMMER, Object));
    }

    public override void TearDown()
    {
        if (Object != null)
        {
            DispatchEvent(new RoomObjectWidgetRequestEvent(
                RoomObjectWidgetRequestEvent.WIDGET_REMOVE_DIMMER, Object));
        }

        base.TearDown();
    }

    private void DispatchColorUpdateEvent()
    {
        if (Object == null)
        {
            return;
        }

        string? itemData = Object.ModelController.GetString("furniture_data");

        if (string.IsNullOrEmpty(itemData))
        {
            return;
        }

        ReadState(itemData);
    }

    private void ReadState(string data)
    {
        if (Object == null)
        {
            return;
        }

        string[] parts = data.Split(',');

        if (parts.Length < 5)
        {
            return;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int state))
        {
            return;
        }

        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int presetId))
        {
            return;
        }

        if (!int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int effectId))
        {
            return;
        }

        string colorStr = parts[3];

        if (colorStr.StartsWith('#'))
        {
            colorStr = colorStr[1..];
        }

        if (!uint.TryParse(colorStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint color))
        {
            return;
        }

        if (!int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int brightness))
        {
            return;
        }

        _dimmerActive = state - 1 > 0;

        DispatchEvent(new RoomObjectDimmerStateUpdateEvent(Object, state - 1, presetId, effectId, color, brightness));
    }
}
