namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureHockeyScoreLogic
public class FurnitureHockeyScoreLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (Object == null)
        {
            base.MouseEvent(mouseEvent, geometry);
            return;
        }

        switch (mouseEvent.Type)
        {
            case "click":
                switch (mouseEvent.SpriteTag)
                {
                    case "inc":
                        DispatchEvent(new RoomObjectStateChangeEvent(
                            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object, 2));
                        return;
                    case "dec":
                        DispatchEvent(new RoomObjectStateChangeEvent(
                            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object, 1));
                        return;
                }
                break;

            case "doubleClick":
                if (mouseEvent.SpriteTag == "off")
                {
                    DispatchEvent(new RoomObjectStateChangeEvent(
                        RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object, 3));
                    return;
                }
                break;
        }

        base.MouseEvent(mouseEvent, geometry);
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        DispatchEvent(new RoomObjectStateChangeEvent(
            RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object));
    }
}
