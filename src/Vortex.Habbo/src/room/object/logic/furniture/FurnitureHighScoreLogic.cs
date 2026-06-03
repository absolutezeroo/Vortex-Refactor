namespace Vortex.Habbo.Room.Object.Logic;

using Vortex.Habbo.Room.Events;
using Vortex.Habbo.Room.Messages;
using Vortex.Room.Events;
using Vortex.Room.Messages;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureHighScoreLogic (class_3432)
public class FurnitureHighScoreLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types =
        [
            RoomObjectWidgetRequestEvent.HIGH_SCORE_DISPLAY,
            RoomObjectWidgetRequestEvent.HIDE_HIGH_SCORE_DISPLAY,
        ];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void ProcessUpdateMessage(RoomObjectUpdateMessage? message)
    {
        base.ProcessUpdateMessage(message);

        if (message is not RoomObjectDataUpdateMessage || Object == null)
        {
            return;
        }

        int state = Object.GetState(0);

        if (state == 0)
        {
            DispatchEvent(new RoomObjectWidgetRequestEvent(
                RoomObjectWidgetRequestEvent.HIDE_HIGH_SCORE_DISPLAY, Object));
        }
        else
        {
            DispatchEvent(new RoomObjectWidgetRequestEvent(
                RoomObjectWidgetRequestEvent.HIGH_SCORE_DISPLAY, Object));
        }
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (mouseEvent.Type == "doubleClick")
        {
            UseObject();

            return;
        }

        base.MouseEvent(mouseEvent, geometry);
    }
}
