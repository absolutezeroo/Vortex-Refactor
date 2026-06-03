namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureGroupForumTerminalLogic (class_3406)
public class FurnitureGroupForumTerminalLogic : FurnitureGuildCustomizedLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.INTERNAL_LINK];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    protected override void OpenContextMenu()
    {
        // No context menu for forum terminal — overrides guild context menu
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        string? guildId = Object.ModelController.GetString("furniture_guild_customized_guild_id");

        if (guildId != null)
        {
            Object.ModelController.SetString("furniture_internal_link", "groupforum/" + guildId);
        }

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.INTERNAL_LINK, Object));
    }
}
