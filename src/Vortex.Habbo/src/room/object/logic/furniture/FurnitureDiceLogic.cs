namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureDiceLogic
public class FurnitureDiceLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectFurnitureActionEvent.DICE_ACTIVATE,
            RoomObjectFurnitureActionEvent.DICE_OFF,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (Object == null)
        {
            return;
        }

        switch (mouseEvent.Type)
        {
            case "doubleClick":
                {
                    int state = Object.GetState(0);

                    if (state is 0 or 100)
                    {
                        DispatchEvent(new RoomObjectFurnitureActionEvent(
                            RoomObjectFurnitureActionEvent.DICE_ACTIVATE, Object));
                    }
                    else
                    {
                        DispatchEvent(new RoomObjectFurnitureActionEvent(
                            RoomObjectFurnitureActionEvent.DICE_OFF, Object));
                    }

                    return;
                }
            case "click":
                {
                    if (mouseEvent.SpriteTag is "activate" or "deactivate")
                    {
                        int state = Object.GetState(0);

                        if (state is 0 or 100)
                        {
                            DispatchEvent(new RoomObjectFurnitureActionEvent(
                                RoomObjectFurnitureActionEvent.DICE_ACTIVATE, Object));
                        }
                        else
                        {
                            DispatchEvent(new RoomObjectFurnitureActionEvent(
                                RoomObjectFurnitureActionEvent.DICE_OFF, Object));
                        }
                    }

                    break;
                }
        }

        base.MouseEvent(mouseEvent, geometry);
    }
}
