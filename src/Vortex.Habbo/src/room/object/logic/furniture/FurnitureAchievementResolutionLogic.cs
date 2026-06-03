namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureAchievementResolutionLogic (class_3408)
public class FurnitureAchievementResolutionLogic : FurnitureBadgeDisplayLogic
{
    private const int NOT_STARTED = 0;
    private const int IN_PROGRESS = 1;
    private const int ACHIEVED = 2;
    private const int FAILED = 3;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_OPEN,
            RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_ENGRAVING,
            RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_FAILED,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        int state = Object.GetState(0);

        switch (state)
        {
            case NOT_STARTED:
                DispatchEvent(new RoomObjectWidgetRequestEvent(
                    RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_OPEN, Object));

                break;
            case IN_PROGRESS:
                break;
            case ACHIEVED:
                DispatchEvent(new RoomObjectWidgetRequestEvent(
                    RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_ENGRAVING, Object));

                break;
            case FAILED:
                DispatchEvent(new RoomObjectWidgetRequestEvent(
                    RoomObjectWidgetRequestEvent.ACHIEVEMENT_RESOLUTION_FAILED, Object));

                break;
        }
    }

    protected override void UpdateBadge()
    {
        if (Object == null)
        {
            return;
        }

        string? badgeCode = Object.ModelController.GetString("furniture_data_1");

        if (badgeCode is not
            { Length: > 0 } or "ACH_0")
        {
            return;
        }

        Object.ModelController.SetString("furniture_badge_code", badgeCode);

        DispatchEvent(new RoomObjectBadgeAssetEvent(
            RoomObjectBadgeAssetEvent.LOAD_BADGE, Object, badgeCode, false));
    }
}
