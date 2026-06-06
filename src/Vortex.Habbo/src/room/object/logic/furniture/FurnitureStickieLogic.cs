namespace Vortex.Habbo.Room.Object.Logic;

using System;

using Events;
using Messages;
using Vortex.Room.Messages;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureStickieLogic
public class FurnitureStickieLogic : FurnitureLogic
{
    private static readonly string[] STICKIE_COLORS = ["9CCEFF", "FF9CFF", "9CFF9C", "FFFF33"];

    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.STICKIE,
            RoomObjectFurnitureActionEvent.STICKIE,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(System.Xml.Linq.XElement? xml)
    {
        base.Initialize(xml);

        Object?.ModelController.SetNumber("furniture_is_stickie", 1);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectItemDataUpdateMessage || Object == null)
        {
            return;
        }

        SetColorIndexFromItemData();
        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.STICKIE, Object));
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectFurnitureActionEvent(
            RoomObjectFurnitureActionEvent.STICKIE, Object));
    }

    private void SetColorIndexFromItemData()
    {
        if (Object == null)
        {
            return;
        }

        string? itemData = Object.ModelController.GetString("furniture_itemdata");

        if (itemData == null || itemData.Length < 6)
        {
            return;
        }

        string colorHex = itemData[..6];

        for (int i = 0; i < STICKIE_COLORS.Length; i++)
        {
            if (!string.Equals(STICKIE_COLORS[i], colorHex, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Object.ModelController.SetNumber("furniture_color", i + 1);

            return;
        }
    }
}
