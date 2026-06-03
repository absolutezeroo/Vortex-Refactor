namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureWelcomeGiftLogic
public class FurnitureWelcomeGiftLogic : FurnitureMultiStateLogic
{
    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (mouseEvent.Type == "doubleClick" && Object != null)
        {
            DispatchEvent(new RoomObjectStateChangeEvent(
                RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object));
        }

        base.MouseEvent(mouseEvent, geometry);
    }
}
