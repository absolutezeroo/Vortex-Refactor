namespace Vortex.Habbo.Room.Object.Logic;

using Events;
using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureClothingChangeLogic
public class FurnitureClothingChangeLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.CLOTHING_CHANGE];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is RoomObjectDataUpdateMessage && Object != null)
        {
            ParseClothingData();
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }
        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.CLOTHING_CHANGE, Object));
    }

    private void ParseClothingData()
    {
        if (Object == null)
        {
            return;
        }

        string? data = Object.ModelController.GetString("furniture_data");

        if (data == null)
        {
            return;
        }

        string[] parts = data.Split(',');

        if (parts.Length < 2)
        {
            return;
        }

        Object.ModelController.SetString("furniture_clothing_boy", parts[0]);
        Object.ModelController.SetString("furniture_clothing_girl", parts[1]);
    }
}
