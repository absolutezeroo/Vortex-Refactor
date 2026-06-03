namespace Vortex.Habbo.Room.Object.Logic;

using System;
using System.Xml.Linq;

using Vortex.Habbo.Room.Events;

/// @see com.sulake.habbo.room.object.logic.furniture.FurnitureEditableRoomLinkLogic
public class FurnitureEditableRoomLinkLogic : FurnitureLogic
{
    private int _resetTime = -1;

    public override string[]? GetEventTypes()
    {
        string[] types = [RoomObjectWidgetRequestEvent.ROOM_LINK];

        return GetAllEventTypes(base.GetEventTypes() ?? [], types);
    }

    public override void Initialize(XElement? xml)
    {
        base.Initialize(xml);

        if (xml == null || Object == null)
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
            Object.ModelController.SetString("furniture_internal_link", link);
        }
    }

    public override void Update(int time)
    {
        base.Update(time);

        if (_resetTime <= 0 || time < _resetTime)
        {
            return;
        }

        SetAnimationState(0);

        _resetTime = -1;
    }

    public override void UseObject()
    {
        if (Object == null)
        {
            return;
        }

        SetAnimationState(1);

        _resetTime = Environment.TickCount + 2500;

        DispatchEvent(new RoomObjectWidgetRequestEvent(
            RoomObjectWidgetRequestEvent.ROOM_LINK, Object));
    }

    private void SetAnimationState(int state)
    {
        Object?.ModelController.SetNumber("furniture_automatic_state_index", state);
    }
}
