namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Habbo.Room.Object.Data;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureBadgeDisplayLogic (class_3407)
public class FurnitureBadgeDisplayLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectBadgeAssetEvent.LOAD_BADGE,
            RoomObjectWidgetRequestEvent.BADGE_DISPLAY_ENGRAVING,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (Object == null)
        {
            return;
        }

        switch (message)
        {
            case RoomObjectDataUpdateMessage dataMsg:
                if (dataMsg.Data is StringArrayStuffData)
                {
                    UpdateBadge();
                }
                break;
            case RoomObjectGroupBadgeUpdateMessage badgeMsg:
                Object.ModelController.SetString("furniture_badge_asset_name", badgeMsg.AssetName);
                break;
        }
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.BADGE_DISPLAY_ENGRAVING, Object));
    }

    protected virtual void UpdateBadge()
    {
        if (Object == null)
        {
            return;
        }

        string? badgeCode = Object.ModelController.GetString("furniture_data_1");

        if (badgeCode is not
            { Length: > 0 })
        {
            return;
        }

        Object.ModelController.SetString("furniture_badge_code", badgeCode);

        DispatchEvent(new RoomObjectBadgeAssetEvent(
            RoomObjectBadgeAssetEvent.LOAD_BADGE, Object, badgeCode, false));
    }
}
