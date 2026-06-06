namespace Vortex.Habbo.Room.Object.Logic;

using Events;
using Messages;
using Data;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureMannequinLogic
public class FurnitureMannequinLogic : FurnitureLogic
{
    private const string KEY_GENDER = "GENDER";
    private const string KEY_FIGURE = "FIGURE";
    private const string KEY_OUTFIT_NAME = "OUTFIT_NAME";

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.MANNEQUIN];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is RoomObjectDataUpdateMessage dataMsg && Object != null)
        {
            SetObjectVariables(dataMsg.Data);
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.MANNEQUIN, Object));
    }

    private void SetObjectVariables(IStuffData? data)
    {
        if (Object == null || data is not MapStuffData mapData)
        {
            return;
        }

        string? gender = mapData.GetValue(KEY_GENDER);

        if (gender != null)
        {
            Object.ModelController.SetString("furniture_mannequin_gender", gender);
        }

        string? figure = mapData.GetValue(KEY_FIGURE);

        if (figure != null)
        {
            Object.ModelController.SetString("furniture_mannequin_figure", figure);
        }

        string? outfitName = mapData.GetValue(KEY_OUTFIT_NAME);

        if (outfitName != null)
        {
            Object.ModelController.SetString("furniture_mannequin_name", outfitName);
        }
    }
}
