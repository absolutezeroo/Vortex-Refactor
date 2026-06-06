namespace Vortex.Habbo.Room.Object.Logic;

using Events;
using Vortex.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureMultiStateLogic
public class FurnitureMultiStateLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON,
            RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        switch (mouseEvent.Type)
        {
            case "rollOver":
                DispatchEvent(new RoomObjectFurnitureActionEvent(
                    RoomObjectFurnitureActionEvent.CURSOR_REQUEST_BUTTON, Object));

                break;
            case "rollOut":
                DispatchEvent(new RoomObjectFurnitureActionEvent(
                    RoomObjectFurnitureActionEvent.CURSOR_REQUEST_ARROW, Object));

                break;
        }

        base.MouseEvent(mouseEvent, geometry);
    }
}
