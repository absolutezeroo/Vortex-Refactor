namespace Vortex.Habbo.Room.Object.Logic;

using System.Xml.Linq;

using Events;
using Vortex.Room.Events;
using Vortex.Room.Utils;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureCounterClockLogic (class_3444)
public class FurnitureCounterClockLogic : FurnitureLogic
{
    private bool _startState;
    private int _frameCounter;

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectStateChangeEvent.ROOM_OBJECT_STATE_CHANGE];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null)
        {
            return;
        }

        XElement? action = xml.Element("action");

        if (action == null)
        {
            return;
        }

        string? link = (string?)action.Attribute("link");

        if (link != null)
        {
            Object?.ModelController.SetString("furniture_internal_link", link);
        }

        string? startStateStr = (string?)action.Attribute("startState");
        _startState = startStateStr == "1";
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (!_startState || _frameCounter < 0)
        {
            return;
        }

        _frameCounter++;

        if (_frameCounter < 20)
        {
            return;
        }

        SetAnimationState(1);

        _frameCounter = -1;
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

    private void SetAnimationState(int state)
    {
        Object?.ModelController.SetNumber("furniture_automatic_state_index", state);
    }
}
