namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

using Vortex.Habbo.Room.Events;
using Vortex.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureFireworksLogic
public class FurnitureFireworksLogic : FurnitureLogic
{
    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE];
        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
        {
            return;
        }

        XElement? particleSystems = xml.Element("particlesystems");

        if (particleSystems != null)
        {
            Object.ModelController.SetString("furniture_fireworks_data", particleSystems.ToString());
        }
    }

    public override void MouseEvent(RoomSpriteMouseEvent mouseEvent, IRoomGeometry geometry)
    {
        if (Object == null)
        {
            base.MouseEvent(mouseEvent, geometry);

            return;
        }

        if (mouseEvent.Type == "doubleClick")
        {
            switch (mouseEvent.SpriteTag)
            {
                case "start_stop":
                    DispatchEvent(new RoomObjectStateChangeEvent(
                        RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object, 1));
                    return;
                case "reset":
                    DispatchEvent(new RoomObjectStateChangeEvent(
                        RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE, Object, 2));
                    return;
            }
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
