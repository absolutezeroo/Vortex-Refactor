namespace Vortex.Habbo.Room.Object.Logic;

using System.Globalization;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Habbo.Room.Object.Data;
using Vortex.Room.Messages;
using Vortex.Room.Object;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureGuildCustomizedLogic
public class FurnitureGuildCustomizedLogic : FurnitureMultiStateLogic
{
    private const int GUILD_ID_KEY = 1;
    private const int BADGE_CODE_KEY = 2;
    private const int COLOR_1_KEY = 3;
    private const int COLOR_2_KEY = 4;

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectBadgeAssetEvent.LOAD_BADGE,
            RoomObjectWidgetRequestEvent.GUILD_FURNI_CONTEXT_MENU,
            RoomObjectWidgetRequestEvent.CLOSE_FURNI_CONTEXT_MENU,
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
                    UpdateGuildId();
                    UpdateGuildBadge();
                    UpdateGuildColors();
                }

                break;
            case RoomObjectGroupBadgeUpdateMessage badgeMsg:
                Object.ModelController.SetString("furniture_guild_customized_asset_name", badgeMsg.AssetName);

                break;
            case RoomObjectSelectedMessage selectedMsg:
                if (selectedMsg.Selected)
                {
                    OpenContextMenu();
                }
                else
                {
                    DispatchEvent(new RoomObjectWidgetRequestEvent(
                        RoomObjectWidgetRequestEvent.CLOSE_FURNI_CONTEXT_MENU, Object));
                }

                break;
        }
    }

    protected virtual void OpenContextMenu()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.GUILD_FURNI_CONTEXT_MENU, Object));
    }

    private void UpdateGuildId()
    {
        string? guildIdStr = Object?.ModelController.GetString("furniture_data_" + GUILD_ID_KEY);

        if (guildIdStr != null)
        {
            Object!.ModelController.SetString("furniture_guild_customized_guild_id", guildIdStr);
        }
    }

    private void UpdateGuildBadge()
    {
        string? badgeCode = Object?.ModelController.GetString("furniture_data_" + BADGE_CODE_KEY);

        if (badgeCode is
            not
            {
                Length: > 0,
            })
        {
            return;
        }

        Object!.ModelController.SetString("furniture_guild_customized_badge_code", badgeCode);

        DispatchEvent(new RoomObjectBadgeAssetEvent(
            RoomObjectBadgeAssetEvent.LOAD_BADGE, Object, badgeCode, true));
    }

    private void UpdateGuildColors()
    {
        if (Object == null)
        {
            return;
        }

        string? color1Str = Object.ModelController.GetString("furniture_data_" + COLOR_1_KEY);

        if (color1Str != null && int.TryParse(color1Str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int color1))
        {
            Object.ModelController.SetNumber("furniture_guild_customized_color_1", color1);
        }

        string? color2Str = Object.ModelController.GetString("furniture_data_" + COLOR_2_KEY);

        if (color2Str != null && int.TryParse(color2Str, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int color2))
        {
            Object.ModelController.SetNumber("furniture_guild_customized_color_2", color2);
        }
    }
}
